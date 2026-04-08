#!/usr/bin/env node

/**
 * Keyword Mode Injector Hook (UserPromptSubmit)
 *
 * 사용자 입력에서 키워드를 감지하고, 해당 모드의 프롬프트를 additionalContext로 주입한다.
 *
 * 지원 키워드:
 *   - tdd          → TDD 모드 (테스트 먼저 작성)
 *   - codereview   → 코드 리뷰 모드
 *   - analyze      → 분석 모드 (탐색 먼저)
 *   - ultrathink   → 심층 사고 모드
 */

async function readStdin() {
  return new Promise((resolve, reject) => {
    let data = '';
    process.stdin.setEncoding('utf-8');
    process.stdin.on('data', chunk => { data += chunk; });
    process.stdin.on('end', () => resolve(data));
    process.stdin.on('error', reject);
  });
}

// 각 모드별 주입 프롬프트
const MODES = {
  tdd: `<tdd-mode>
[TDD 모드 활성화]
실용적인 경우 테스트를 먼저 작성하고, 올바른 이유로 실패하는지 확인한 후, 최소한의 수정으로 구현하고 재검증한다.
Unity에서는: EditMode/PlayMode 테스트를 먼저 작성하거나 기존 테스트를 먼저 확인한다.
</tdd-mode>

---
`,

  analyze: `<analyze-mode>
[분석 모드 활성화]
실행 전에 먼저 컨텍스트를 수집한다:
- 관련 코드 경로를 먼저 탐색
- 동작하는 것과 동작하지 않는 것을 비교
- 변경을 제안하기 전에 발견한 내용을 종합
</analyze-mode>

---
`,

  ultrathink: `<think-mode>
[ULTRATHINK 모드 활성화] — 심층 추론 활성화.

다음을 통해 충분한 시간을 갖는다:
1. 여러 각도에서 문제를 철저히 분석
2. 엣지 케이스와 잠재적 문제 고려
3. 각 접근 방식의 시사점을 생각
4. 행동하기 전에 단계별로 추론

가장 철저하고 잘 추론된 응답을 제공한다.
</think-mode>

---
`,
};

/**
 * 코드블록, URL, 파일 경로, XML 태그 등을 제거하여 키워드 오탐을 방지한다.
 */
function sanitizePrompt(text) {
  return text
    .replace(/```[\s\S]*?```/g, '')    // 코드 블록 제거
    .replace(/`[^`]+`/g, '')           // 인라인 코드 제거
    .replace(/https?:\/\/\S+/g, '')    // URL 제거
    .replace(/[a-zA-Z]:[/\\]\S+/g, '') // Windows 경로 제거
    .replace(/\/[\w/.-]+\.\w+/g, '')   // Unix 경로 제거
    .replace(/<[^>]+>/g, '')           // XML/HTML 태그 제거
    .toLowerCase();
}

/**
 * 정보성 질문 필터 — "tdd가 뭐야?" 같은 질문에서 모드 활성화를 방지한다.
 */
function isInformationalQuestion(text, keyword) {
  const infoPatterns = [
    /\b(what is|what's|what are|explain|how to use|how does|뭐야|뭔가요|설명|어떻게 사용|알려줘)\b/,
  ];
  const keywordPos = text.indexOf(keyword);
  if (keywordPos === -1) return false;
  const surrounding = text.slice(Math.max(0, keywordPos - 50), keywordPos + 50);
  return infoPatterns.some(p => p.test(surrounding));
}

/**
 * 키워드가 실제로 액션 요청인지 확인한다.
 */
function hasActionableKeyword(text, keyword) {
  if (!text.includes(keyword)) return false;
  return !isInformationalQuestion(text, keyword);
}

async function main() {
  try {
    const input = await readStdin();
    const data = JSON.parse(input);
    const rawPrompt = data.prompt || data.message || '';
    const sanitized = sanitizePrompt(rawPrompt);

    // 매칭된 모드 수집
    const matchedModes = [];
    for (const [keyword, message] of Object.entries(MODES)) {
      if (hasActionableKeyword(sanitized, keyword)) {
        matchedModes.push(message);
      }
    }

    if (matchedModes.length === 0) {
      console.log(JSON.stringify({ continue: true }));
      return;
    }

    const additionalContext = matchedModes.join('\n');
    console.log(JSON.stringify({
      continue: true,
      hookSpecificOutput: {
        hookEventName: 'UserPromptSubmit',
        additionalContext,
      },
    }));
  } catch {
    console.log(JSON.stringify({ continue: true }));
  }
}

main();
