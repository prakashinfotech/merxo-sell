"""Runs ng build and ng test (headless) for the Angular frontend."""

import subprocess
import time
import os
import re
from dataclasses import dataclass


@dataclass
class RunnerResult:
    name: str
    passed: bool
    duration: float
    output: str
    error: str = ""
    tests_total: int = 0
    tests_passed: int = 0
    tests_failed: int = 0
    tests_skipped: int = 0


def _run(cmd: list, cwd: str, timeout: int) -> tuple[int, str, str]:
    try:
        proc = subprocess.run(
            cmd,
            cwd=cwd,
            capture_output=True,
            text=True,
            timeout=timeout,
            shell=True,
        )
        return proc.returncode, proc.stdout, proc.stderr
    except subprocess.TimeoutExpired:
        return -1, "", f"Command timed out after {timeout}s"
    except FileNotFoundError as e:
        return -1, "", str(e)


def _parse_karma_summary(output: str) -> tuple[int, int, int]:
    """Extract passed/failed/skipped from Karma console output."""
    passed = failed = skipped = 0
    # Karma prints: "X specs, Y failures"
    m = re.search(r"(\d+)\s+specs?,\s+(\d+)\s+failure", output)
    if m:
        total_specs = int(m.group(1))
        failed = int(m.group(2))
        passed = total_specs - failed
    return passed + failed, passed, failed


def run_frontend_build(frontend_path: str, timeout: int = 180) -> RunnerResult:
    abs_path = os.path.abspath(frontend_path)
    start = time.time()
    code, stdout, stderr = _run(
        "npx ng build --configuration production",
        cwd=abs_path,
        timeout=timeout,
    )
    duration = time.time() - start
    return RunnerResult(
        name="Frontend Build (ng build --production)",
        passed=(code == 0),
        duration=duration,
        output=stdout,
        error=stderr if code != 0 else "",
    )


def run_frontend_tests(frontend_path: str, timeout: int = 120) -> RunnerResult:
    abs_path = os.path.abspath(frontend_path)
    start = time.time()
    # --no-progress keeps output clean in CI; --watch=false exits after one run
    code, stdout, stderr = _run(
        "npx ng test --watch=false --browsers=ChromeHeadless --no-progress",
        cwd=abs_path,
        timeout=timeout,
    )
    duration = time.time() - start
    total, passed, failed = _parse_karma_summary(stdout + stderr)
    return RunnerResult(
        name="Frontend Unit Tests (Karma/Jasmine)",
        passed=(code == 0),
        duration=duration,
        output=stdout,
        error=stderr if code != 0 else "",
        tests_total=total,
        tests_passed=passed,
        tests_failed=failed,
    )
