---
name: epos-gpt-image
description: 에포스에서 지피티(ChatGPT 앱 안의 명령줄 도구)로 기준 그림 · 컨셉 그림을 만들 때 쓴다 — 일감 크기에 맞는 작은 모델로 부르고, 받은 그림을 스스로 열어 점검한 뒤에만 넘기고, 그림 기록에 한 줄 남긴다. 지피티에게는 그림 만들기만 맡긴다.
---

# 지피티 그림 부르기

> 지위: 🔵 원리(제제 2026-11-25 — 지피티 = 그림 만들기에 집중 · 범용 역할 ✗ · 작은 일에 큰 모델 ✗ · 넘기기 전 자체 검토) · 모델 · 노력 · 점검 항목은 ⚑ · 정본 = `Docs/60_AI개발/다른-AI-쓰는-법-한장.md`.

## 1. 부르는 법
- 도구: `/Applications/ChatGPT.app/Contents/Resources/codex-cli/bin/codex` — ChatGPT 앱 안에 들어 있다. 따로 깔지 않는다.
- **모델과 노력을 늘 적어서 부른다.** 제제의 기본 설정(옛 모델 · 가장 높은 노력)을 그대로 쓰면 사용량이 금방 찬다.

| 일감 | 모델 ⚑ | 노력 ⚑ |
|---|---|---|
| 지시문이 정해진 그림 한 장 · 같은 틀로 한 장 더 | `gpt-6-luna` | `low` |
| 지시문을 새로 짜야 하는 그림(새 인물 · 새 틀) | `gpt-6.2-sol` | `low` |
| 큰 판단 | 쓰지 않는다 | 판단은 클로드 팀장 |

```bash
cd "<그림을 둘 폴더>" && /Applications/ChatGPT.app/Contents/Resources/codex-cli/bin/codex exec -m gpt-6-luna -c model_reasoning_effort='"low"' -s workspace-write --skip-git-repo-check "<지시문>. Generate exactly one image with the image generation tool and save it as <파일이름>.png in the current folder. Do nothing else."
```
- 그림은 `~/.codex/generated_images/<번호>/`에도 남는다. 폴더에 파일이 없으면 거기서 가장 새 파일을 가져온다.
- 여러 장이 필요하면 **한 번에 묶어서** 부른다 — 부를 때마다 기본 짐(11-25 실측: «OK» 한 마디에도 약 1만 3천 토큰)이 붙는다.
- ⚑ 확인 못 한 것(11-25): 가장 작은 모델이 그림 도구를 부르는지. 첫 그림에서 확인하고 이 줄을 고친다. 부르지 않으면 `gpt-6.2-sol`로 올린다.

## 2. 지시문 틀 (11-22 그림 기록에서)
- 영어로 쓴다: `full-body fantasy knight, standing, front view, (인물 묘사), plain flat light-gray background, simple clean shapes`
- 요령(⚑ 할 일 3-4): 단색 바탕에 물체 하나 · 그림에 글자 · 숫자를 넣지 않는다 · 고칠 때는 그 부분만 다시
- 금지선: 참고작이나 다른 게임의 이름 · 그림을 지시문에 넣지 않는다(`CLAUDE.md` 2절).

## 3. 넘기기 전 자체 점검 (한 장마다 직접 열어 본다)
1. 지시한 것(무기 · 머리 · 색 · 자세)이 다 있나
2. 바탕이 단색이고 물체가 하나인가
3. 글자 · 숫자 · 서명 · 물 자국 무늬(워터마크)가 없나
4. 팀컬러(아군 파랑 · 적 빨강 · 지원 초록 · 중립 노랑)와 어긋나지 않나
5. 손 · 무기 · 얼굴이 깨지지 않았나

하나라도 어긋나면 **그 부분만 고친 지시문으로 한 번만** 다시 만든다. 두 번째도 어긋나면 멈추고 보고한다(사용량을 지킨다).

## 4. 기록
그림마다 그림 기록(점 그림 판 `pixel/docs/그림-기록.md`)에 한 줄: 날짜 · 결과 · 도구(모델 · 노력) · 지시문 · 기준 그림 · 후처리 · 자체 점검 결과 · 다시 만든 횟수.
