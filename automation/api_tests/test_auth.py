"""Auth endpoint tests - register, login, token validation."""

import time
import uuid
from .base import ApiTestBase, TestSuiteResult, TestCase


class AuthTests(ApiTestBase):
    def __init__(self, base_url: str, credentials: dict, timeout: int = 30):
        super().__init__(base_url, timeout)
        self.credentials = credentials
        # Populated by _test_register_new_buyer so login test can reuse them
        self._test_buyer_email: str = ""
        self._test_buyer_password: str = "AutoTest@123"

    def run(self) -> TestSuiteResult:
        self._results.suite_name = "Auth API Tests"

        # Register first so buyer-login test has valid credentials
        self._test_register_new_buyer()
        self._test_buyer_login()
        self._test_admin_login()
        self._test_seller_login()
        self._test_invalid_login()
        self._test_protected_route_without_token()
        self._test_protected_route_with_token()

        return self._results

    def _test_register_new_buyer(self):
        self._test_buyer_email = f"test_{uuid.uuid4().hex[:8]}@auto.test"
        t = time.time()
        r, err = self._request("POST", "/auth/register", json={
            "fullName": "Auto Tester",
            "email": self._test_buyer_email,
            "password": self._test_buyer_password,
            "confirmPassword": self._test_buyer_password,
        })
        self._assert("POST /auth/register - new buyer account", t, r, err, expected_status=200)

    def _test_buyer_login(self):
        """Log in with the freshly-registered test buyer."""
        if not self._test_buyer_email:
            self._results.cases.append(TestCase(
                name="POST /auth/login - buyer credentials",
                passed=False, duration=0,
                error="Register step failed; skipping buyer login test",
            ))
            return
        t = time.time()
        r, err = self._request("POST", "/auth/login", json={
            "email": self._test_buyer_email,
            "password": self._test_buyer_password,
        })
        self._assert(
            "POST /auth/login - buyer credentials",
            t, r, err, expected_status=200,
            extra_check=lambda resp: "" if (resp.json().get("data") or {}).get("accessToken")
                else "No accessToken in response",
        )

    def _test_admin_login(self):
        creds = self.credentials.get("admin", {})
        t = time.time()
        r, err = self._request("POST", "/auth/admin/login", json={
            "email": creds.get("email"),
            "password": creds.get("password"),
        })
        self._assert("POST /auth/admin/login - admin credentials", t, r, err, expected_status=200)

    def _test_seller_login(self):
        creds = self.credentials.get("seller", {})
        t = time.time()
        r, err = self._request("POST", "/auth/seller/login", json={
            "email": creds.get("email"),
            "password": creds.get("password"),
        })
        self._assert("POST /auth/seller/login - seller credentials", t, r, err, expected_status=200)

    def _test_invalid_login(self):
        t = time.time()
        r, err = self._request("POST", "/auth/login", json={
            "email": "nobody@nowhere.com",
            "password": "WrongPassword!999",
        })
        self._assert("POST /auth/login - invalid creds returns 401", t, r, err, expected_status=401)

    def _test_protected_route_without_token(self):
        self.clear_auth()
        t = time.time()
        r, err = self._request("GET", "/profile")
        self._assert("GET /profile without token returns 401", t, r, err, expected_status=401)

    def _test_protected_route_with_token(self):
        # Use freshly-registered buyer; fall back to admin if needed
        ok = False
        if self._test_buyer_email:
            ok = self.authenticate(self._test_buyer_email, self._test_buyer_password)
        if not ok:
            admin = self.credentials.get("admin", {})
            ok = self.authenticate(admin.get("email"), admin.get("password"), endpoint="/auth/admin/login")
        if not ok:
            self._results.cases.append(TestCase(
                name="GET /profile with valid token returns 200",
                passed=False, duration=0,
                error="Could not obtain any auth token",
            ))
            return
        t = time.time()
        r, err = self._request("GET", "/profile")
        self._assert("GET /profile with valid token returns 200", t, r, err, expected_status=200)
        self.clear_auth()
