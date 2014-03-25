#!/bin/bash
echo 'foobaz'
find ./build/Debug/ -name "*tests.dll" -exec ../lib/NUnit.Runners/tools/nunit-console-x86.exe {} \;