FROM mcr.microsoft.com/devcontainers/dotnet:1-8.0

ARG ACTIONLINT_VERSION
ARG SHELLCHECK_VERSION
ARG YQ_VERSION

RUN set -eux; \
    arch="$(dpkg --print-architecture)"; \
    case "$arch" in \
      amd64) sc_arch=x86_64 ;; \
      arm64) sc_arch=aarch64 ;; \
      *) echo "unsupported arch: $arch" >&2; exit 1 ;; \
    esac; \
    curl -fsSL "https://github.com/rhysd/actionlint/releases/download/v${ACTIONLINT_VERSION}/actionlint_${ACTIONLINT_VERSION}_linux_${arch}.tar.gz" \
      | tar -xz -C /usr/local/bin actionlint; \
    curl -fsSL "https://github.com/koalaman/shellcheck/releases/download/v${SHELLCHECK_VERSION}/shellcheck-v${SHELLCHECK_VERSION}.linux.${sc_arch}.tar.xz" \
      | tar -xJ -C /usr/local/bin --strip-components=1 "shellcheck-v${SHELLCHECK_VERSION}/shellcheck"; \
    curl -fsSL -o /usr/local/bin/yq "https://github.com/mikefarah/yq/releases/download/v${YQ_VERSION}/yq_linux_${arch}"; \
    chmod +x /usr/local/bin/yq; \
    actionlint --version; shellcheck --version | sed -n '2p'; yq --version
