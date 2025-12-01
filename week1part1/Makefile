# Targets
.PHONY: build test run ide

ide:
	code .

build:
	npm run build
	chmod +x dist/main.mjs

test:
	npm run test

run:
	npm run start

publish: build
	npm publish --access public

check-updates:
	ncu
