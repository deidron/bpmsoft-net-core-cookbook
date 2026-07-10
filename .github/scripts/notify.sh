set -euo pipefail

# Sends a deploy status notification to Telegram.
# Usage: notify.sh <success|failure>
# Context comes from env: TG_TOKEN, TG_CHAT, DEPLOY_BRANCH, DEPLOY_ENV, RUN_URL.

status="${1:?usage: notify.sh <success|failure>}"

# Skip silently when Telegram isn't configured — keeps the pipeline green for
# forks and setups without notification secrets, instead of failing the job.
if [[ -z "${TG_TOKEN:-}" || -z "${TG_CHAT:-}" ]]; then
  echo "Telegram secrets not set — skipping notification"
  exit 0
fi

case "$status" in
  success) icon="✅"; word="succeeded" ;;
  failure) icon="❌"; word="failed" ;;
  *)       icon="ℹ️"; word="$status" ;;
esac

text="${icon} *Deploy ${word}*
Branch: \`${DEPLOY_BRANCH:-?}\`
Env: \`${DEPLOY_ENV:-?}\`
[View logs](${RUN_URL:-})"

# A failed notification must not fail the deploy job — warn and move on.
curl -sS --fail "https://api.telegram.org/bot${TG_TOKEN}/sendMessage" \
  -d chat_id="${TG_CHAT}" \
  -d parse_mode=Markdown \
  --data-urlencode text="${text}" \
  || echo "::warning::Telegram notification failed to send"
