"""Smoke / health-check tests - run first before anything else."""

import time
from .base import ApiTestBase, TestSuiteResult


class HealthTests(ApiTestBase):
    def run(self) -> TestSuiteResult:
        self._results.suite_name = "Smoke / Health Checks"

        # 1. Swagger UI reachable (strips /api suffix)
        swagger_url = self.base_url.replace("/api", "")
        t = time.time()
        r, err = self._request("GET", "")  # hits /api
        # We just need a non-connection-error response (any 2xx or 4xx is fine)
        passed = r is not None
        from .base import TestCase
        tc = TestCase(
            name="API base URL reachable",
            passed=passed,
            duration=time.time() - t,
            status_code=r.status_code if r else 0,
            error=err,
        )
        self._results.cases.append(tc)

        # 2. Public products endpoint (no auth needed)
        t = time.time()
        r, err = self._request("GET", "/products?pageNumber=1&pageSize=1")
        self._assert("GET /api/products returns 200", t, r, err, expected_status=200)

        # 3. Public categories endpoint
        t = time.time()
        r, err = self._request("GET", "/categories")
        self._assert("GET /api/categories returns 200", t, r, err, expected_status=200)

        # 4. Public currencies endpoint
        t = time.time()
        r, err = self._request("GET", "/currencies")
        self._assert("GET /api/currencies returns 200", t, r, err, expected_status=200)

        # 5. Public offer banners endpoint
        t = time.time()
        r, err = self._request("GET", "/offer-banners")
        self._assert("GET /api/offer-banners returns 200", t, r, err, expected_status=200)

        # 6. Unknown route returns 404 not 500
        t = time.time()
        r, err = self._request("GET", "/nonexistent-route-xyz")
        if r is not None:
            passed = r.status_code == 404
            from .base import TestCase
            tc = TestCase(
                name="Unknown route returns 404 (not 500)",
                passed=passed,
                duration=time.time() - t,
                status_code=r.status_code,
                error="" if passed else f"Got {r.status_code}",
            )
            self._results.cases.append(tc)

        return self._results
