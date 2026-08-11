"""Shared base class for all API test modules."""

import time
import requests
from dataclasses import dataclass, field
from typing import Optional


@dataclass
class TestCase:
    name: str
    passed: bool
    duration: float
    status_code: int = 0
    error: str = ""
    detail: str = ""


@dataclass
class TestSuiteResult:
    suite_name: str
    cases: list = field(default_factory=list)

    @property
    def passed(self) -> bool:
        return all(c.passed for c in self.cases)

    @property
    def total(self) -> int:
        return len(self.cases)

    @property
    def num_passed(self) -> int:
        return sum(1 for c in self.cases if c.passed)

    @property
    def num_failed(self) -> int:
        return self.total - self.num_passed

    @property
    def duration(self) -> float:
        return sum(c.duration for c in self.cases)


class ApiTestBase:
    def __init__(self, base_url: str, timeout: int = 30):
        self.base_url = base_url.rstrip("/")
        self.timeout = timeout
        self.session = requests.Session()
        self.session.headers.update({"Content-Type": "application/json"})
        self._results: TestSuiteResult = TestSuiteResult(suite_name=self.__class__.__name__)

    # ------------------------------------------------------------------
    # Auth helpers
    # ------------------------------------------------------------------

    def authenticate(self, email: str, password: str, endpoint: str = "/auth/login") -> bool:
        """Obtain a JWT and store it in session headers. Returns True on success."""
        try:
            r = self.session.post(
                f"{self.base_url}{endpoint}",
                json={"email": email, "password": password},
                timeout=self.timeout,
            )
            if r.status_code == 200:
                data = r.json()
                nested = data.get("data") or {}
                token = (
                    nested.get("accessToken")
                    or nested.get("token")
                    or data.get("accessToken")
                    or data.get("token")
                )
                if token:
                    self.session.headers["Authorization"] = f"Bearer {token}"
                    return True
        except Exception:
            pass
        return False

    def clear_auth(self):
        self.session.headers.pop("Authorization", None)

    # ------------------------------------------------------------------
    # Request helper
    # ------------------------------------------------------------------

    def _request(self, method: str, path: str, **kwargs) -> tuple[Optional[requests.Response], str]:
        url = f"{self.base_url}{path}"
        try:
            r = self.session.request(method, url, timeout=self.timeout, **kwargs)
            return r, ""
        except requests.ConnectionError:
            return None, f"Connection refused - is the API running at {self.base_url}?"
        except requests.Timeout:
            return None, f"Request timed out after {self.timeout}s"
        except Exception as e:
            return None, str(e)

    # ------------------------------------------------------------------
    # Assertion helpers
    # ------------------------------------------------------------------

    def _assert(
        self,
        name: str,
        start: float,
        response: Optional[requests.Response],
        error: str,
        expected_status: int,
        extra_check=None,
    ) -> TestCase:
        duration = time.time() - start
        if response is None:
            tc = TestCase(name=name, passed=False, duration=duration, error=error)
        elif response.status_code != expected_status:
            tc = TestCase(
                name=name,
                passed=False,
                duration=duration,
                status_code=response.status_code,
                error=f"Expected {expected_status}, got {response.status_code}",
                detail=response.text[:300],
            )
        elif extra_check:
            check_error = extra_check(response)
            tc = TestCase(
                name=name,
                passed=(check_error == ""),
                duration=duration,
                status_code=response.status_code,
                error=check_error,
            )
        else:
            tc = TestCase(name=name, passed=True, duration=duration, status_code=response.status_code)
        self._results.cases.append(tc)
        return tc

    def run(self) -> TestSuiteResult:
        raise NotImplementedError
