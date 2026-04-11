#!/usr/bin/env node

/**
 * Context Guard Hook (Stop)
 *
 * 컨텍스트 사용률이 임계값(기본 75%)을 초과하면 /compact 실행을 권고하며 Stop을 차단한다.
 *
 * 안전 규칙:
 *   - context_limit으로 인한 Stop은 절대 차단하지 않음 (compaction deadlock 방지)
 *   - 사용자 중단(Ctrl+C)은 차단하지 않음
 *   - 세션당 최대 2회만 차단 (무한 루프 방지)
 *   - 에러 발생 시 항상 통과 (훅 실패로 인한 차단 없음)
 */

import { existsSync, readFileSync, writeFileSync, mkdirSync, statSync, openSync, readSync, closeSync } from 'node:fs';
import { join } from 'node:path';
import { homedir, tmpdir } from 'node:os';

const THRESHOLD = parseInt(process.env.CONTEXT_GUARD_THRESHOLD || '75', 10);
const CRITICAL_THRESHOLD = 95;
const MAX_BLOCKS = 2;
const SESSION_ID_PATTERN = /^[a-zA-Z0-9][a-zA-Z0-9_-]{0,255}$/;

function getClaudeConfigDir() {
  return process.env.CLAUDE_CONFIG_DIR || join(homedir(), '.claude');
}

async function readStdin() {
  return new Promise((resolve, reject) => {
    let data = '';
    process.stdin.setEncoding('utf-8');
    process.stdin.on('data', chunk => { data += chunk; });
    process.stdin.on('end', () => resolve(data));
    process.stdin.on('error', reject);
  });
}

function isContextLimitStop(data) {
  const reasons = [
    data.stop_reason, data.stopReason,
    data.end_turn_reason, data.endTurnReason, data.reason,
  ]
    .filter(v => typeof v === 'string' && v.trim().length > 0)
    .map(v => v.toLowerCase().replace(/[\s-]+/g, '_'));

  const contextPatterns = [
    'context_limit', 'context_window', 'context_exceeded',
    'context_full', 'max_context', 'token_limit',
    'max_tokens', 'conversation_too_long', 'input_too_long',
  ];
  return reasons.some(r => contextPatterns.some(p => r.includes(p)));
}

function isUserAbort(data) {
  if (data.user_requested || data.userRequested) return true;
  const reason = (data.stop_reason || data.stopReason || '').toLowerCase();
  return ['aborted', 'abort', 'cancel', 'interrupt'].includes(reason) ||
    ['user_cancel', 'user_interrupt', 'ctrl_c', 'manual_stop'].some(p => reason.includes(p));
}

function estimateContextPercent(transcriptPath) {
  if (!transcriptPath) return 0;
  let fd = -1;
  try {
    const stat = statSync(transcriptPath);
    if (stat.size === 0) return 0;
    fd = openSync(transcriptPath, 'r');
    const readSize = Math.min(4096, stat.size);
    const buf = Buffer.alloc(readSize);
    readSync(fd, buf, 0, readSize, stat.size - readSize);
    closeSync(fd);
    fd = -1;
    const tail = buf.toString('utf-8');
    const windowMatch = tail.match(/"context_window"\s{0,5}:\s{0,5}(\d+)/g);
    const inputMatch = tail.match(/"input_tokens"\s{0,5}:\s{0,5}(\d+)/g);
    if (!windowMatch || !inputMatch) return 0;
    const lastWindow = parseInt(windowMatch[windowMatch.length - 1].match(/(\d+)/)[1], 10);
    const lastInput = parseInt(inputMatch[inputMatch.length - 1].match(/(\d+)/)[1], 10);
    if (lastWindow === 0) return 0;
    return Math.round((lastInput / lastWindow) * 100);
  } catch {
    return 0;
  } finally {
    if (fd !== -1) try { closeSync(fd); } catch { /* ignore */ }
  }
}

function getGuardFilePath(sessionId) {
  const guardDir = join(tmpdir(), '.claude-context-guards');
  try {
    mkdirSync(guardDir, { recursive: true });
  } catch (err) {
    if (err?.code !== 'EEXIST') throw err;
  }
  return join(guardDir, `context-guard-${sessionId}.json`);
}

function getBlockCount(sessionId) {
  if (!sessionId || !SESSION_ID_PATTERN.test(sessionId)) return 0;
  try {
    const guardFile = getGuardFilePath(sessionId);
    if (existsSync(guardFile)) {
      return JSON.parse(readFileSync(guardFile, 'utf-8')).blockCount || 0;
    }
  } catch { /* ignore */ }
  return 0;
}

function incrementBlockCount(sessionId) {
  if (!sessionId || !SESSION_ID_PATTERN.test(sessionId)) return;
  try {
    const guardFile = getGuardFilePath(sessionId);
    let count = 0;
    if (existsSync(guardFile)) {
      count = JSON.parse(readFileSync(guardFile, 'utf-8')).blockCount || 0;
    }
    writeFileSync(guardFile, JSON.stringify({ blockCount: count + 1 }));
  } catch { /* ignore */ }
}

async function main() {
  try {
    const input = await readStdin();
    const data = JSON.parse(input);

    if (isContextLimitStop(data)) {
      console.log(JSON.stringify({ continue: true, suppressOutput: true }));
      return;
    }
    if (isUserAbort(data)) {
      console.log(JSON.stringify({ continue: true, suppressOutput: true }));
      return;
    }

    const sessionId = data.session_id || data.sessionId || '';
    const transcriptPath = data.transcript_path || data.transcriptPath || '';
    const pct = estimateContextPercent(transcriptPath);

    if (pct >= CRITICAL_THRESHOLD) {
      // 95% 이상이면 차단해도 소용없음 — 통과
      console.log(JSON.stringify({ continue: true, suppressOutput: true }));
      return;
    }

    if (pct >= THRESHOLD) {
      const blockCount = getBlockCount(sessionId);
      if (blockCount >= MAX_BLOCKS) {
        // 이미 충분히 경고했음 — 통과
        console.log(JSON.stringify({ continue: true, suppressOutput: true }));
        return;
      }
      incrementBlockCount(sessionId);
      const severity = pct >= 90 ? 'CRITICAL' : 'WARNING';
      console.log(JSON.stringify({
        continue: false,
        decision: 'block',
        reason: `[Context Guard ${severity}] 컨텍스트 ${pct}% 사용 중 (임계값: ${THRESHOLD}%). ` +
          `/compact 실행 후 계속하세요. (차단 ${blockCount + 1}/${MAX_BLOCKS}회)`
      }));
      return;
    }

    console.log(JSON.stringify({ continue: true, suppressOutput: true }));
  } catch {
    // 에러 시 항상 통과
    console.log(JSON.stringify({ continue: true, suppressOutput: true }));
  }
}

main();
