#!/bin/bash
find ./src/build/Debug/ -name "*.dll" -exec ./lib/NUnit.Runners/tools/nunit-console-x86.exe {} \;

