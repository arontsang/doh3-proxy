#!/usr/bin/env bash
set -e

apt-get update
apt-get install -y clang llvm binutils-aarch64-linux-gnu gcc-aarch64-linux-gnu binutils-x86-64-linux-gnu gcc-x86-64-linux-gnu

#rm /etc/apt/sources.list.d/ubuntu.sources -rf
if [ "$BUILDARCH" != "arm64" ]; then
cat > /etc/apt/sources.list.d/arm64.list <<EOF
deb [arch=arm64] http://ports.ubuntu.com/ubuntu-ports/ noble main restricted
deb [arch=arm64] http://ports.ubuntu.com/ubuntu-ports/ noble-updates main restricted
deb [arch=arm64] http://ports.ubuntu.com/ubuntu-ports/ noble-backports main restricted universe multiverse
EOF
fi
#if [ "$BUILDARCH" != "amd64" ]; then
#cat > /etc/apt/sources.list.d/amd64.list <<EOF
#deb [arch=amd64] http://archive.ubuntu.com/ubuntu/ noble main restricted
#deb [arch=amd64] http://archive.ubuntu.com/ubuntu/ noble-updates main restricted
#deb [arch=amd64] http://archive.ubuntu.com/ubuntu/ noble-backports main restricted universe multiverse
#EOF
#fi

set +e
dpkg --add-architecture arm64
dpkg --add-architecture amd64
apt-get update || true
set -e
case $TARGETARCH in
amd64) apt-get install -y zlib1g-dev:amd64 ;;
arm64) apt-get install -y zlib1g-dev:arm64 ;;
esac
rm -rf /var/lib/apt/lists/*