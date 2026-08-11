"""Runs dotnet build and dotnet test for the backend and collects results."""

import subprocess
import time
import os
from dataclasses import dataclass, field
from typing import List


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


def _run(cmd: List[str], cwd: str, timeout: int) -> tuple[int, str, str]:
    try:
        proc = subprocess.run(
            cmd,
            cwd=cwd,
            capture_output=True,
            text=True,
            timeout=timeout,
        )
        return proc.returncode, proc.stdout, proc.stderr
    except subprocess.TimeoutExpired:
        return -1, "", f"Command timed out after {timeout}s"
    except FileNotFoundError as e:
        return -1, "", str(e)


def _parse_dotnet_test_summary(output: str) -> tuple[int, int, int, int]:
    """Extract total/passed/failed/skipped from dotnet test output.

    Handles the two common formats:
      1. Old style:  Total tests: 106   Passed: 104   Failed: 2
      2. New style:  Failed!  - Failed:     2, Passed:   104, Skipped:   0, Total:   106, ...
    """
    import re
    total = passed = failed = skipped = 0

    for line in output.splitlines():
        stripped = line.strip()

        # New-style summary: "Failed!  - Failed:  2, Passed: 104, Skipped:  0, Total: 106"
        # or                 "Passed!  - Failed:  0, Passed: 106, Skipped:  0, Total: 106"
        m = re.search(
            r"Failed:\s*(\d+),\s*Passed:\s*(\d+),\s*Skipped:\s*(\d+),\s*Total:\s*(\d+)",
            stripped,
        )
        if m:
            failed   = int(m.group(1))
            passed   = int(m.group(2))
            skipped  = int(m.group(3))
            total    = int(m.group(4))
            break

        # Old-style: individual key: value pairs on separate lines
        if "Total tests:" in stripped:
            try:
                total = int(re.search(r"Total tests:\s*(\d+)", stripped).group(1))
            except (ValueError, AttributeError):
                pass
        elif "Passed:" in stripped and "Failed:" not in stripped:
            try:
                passed = int(re.search(r"Passed:\s*(\d+)", stripped).group(1))
            except (ValueError, AttributeError):
                pass
        elif "Failed:" in stripped and "Passed:" not in stripped:
            try:
                failed = int(re.search(r"Failed:\s*(\d+)", stripped).group(1))
            except (ValueError, AttributeError):
                pass

    if not total:
        total = passed + failed + skipped
    return total, passed, failed, skipped


def run_backend_build(project_path: str, timeout: int = 120) -> RunnerResult:
    abs_path = os.path.abspath(project_path)
    start = time.time()
    code, stdout, stderr = _run(
        ["dotnet", "build", "--configuration", "Release", "--no-incremental"],
        cwd=abs_path,
        timeout=timeout,
    )
    duration = time.time() - start
    return RunnerResult(
        name="Backend Build (dotnet build)",
        passed=(code == 0),
        duration=duration,
        output=stdout,
        error=stderr if code != 0 else "",
    )


def run_backend_tests(tests_project_path: str, timeout: int = 300) -> RunnerResult:
    abs_path = os.path.abspath(tests_project_path)
    start = time.time()
    code, stdout, stderr = _run(
        [
            "dotnet", "test",
            "--logger", "console;verbosity=normal",
        ],
        cwd=abs_path,
        timeout=timeout,
    )
    duration = time.time() - start
    total, passed, failed, skipped = _parse_dotnet_test_summary(stdout + stderr)
    return RunnerResult(
        name="Backend Unit + Integration Tests (xUnit)",
        passed=(code == 0),
        duration=duration,
        output=stdout,
        error=stderr if code != 0 else "",
        tests_total=total,
        tests_passed=passed,
        tests_failed=failed,
        tests_skipped=skipped,
    )
