#!/bin/bash

ARCH=$(uname -m)

if [ "$ARCH" == "x86_64" ]; then 
  ARCH="amd64" 
fi

VERSION=$(git describe --tags $(git rev-list --tags --max-count=1))

DOCKER_TAG="$VERSION-$ARCH"

echo "Building docker tag: $DOCKER_TAG"

docker build --no-cache -t danilbrenner/bw-vault-digest:$DOCKER_TAG .
docker tag danilbrenner/bw-vault-digest:$DOCKER_TAG danilbrenner/bw-vault-digest:$ARCH 
docker push danilbrenner/bw-vault-digest:$DOCKER_TAG 
docker push danilbrenner/bw-vault-digest:$ARCH