"""UI tests for login, register, and logout flows."""

import time
import uuid
from .base import UiTestBase, UiTestSuiteResult


class AuthUiTests(UiTestBase):
    def __init__(self, frontend_url: str, credentials: dict,
                 headless: bool = False, slow_mo: int = 300):
        super().__init__(frontend_url, headless, slow_mo)
        self.credentials = credentials
        self._registered_email = ""
        self._registered_password = "AutoUi@123"

    def run(self) -> UiTestSuiteResult:
        self._results.suite_name = "Auth UI Tests"

        self._test_login_page_loads()
        self._test_login_form_elements()
        self._test_invalid_login_shows_error()
        self._test_register_page_loads()
        self._test_register_new_user()
        self._test_login_with_registered_user()
        self._test_profile_link_visible_after_login()
        self._test_logout()

        return self._results

    def _test_login_page_loads(self):
        start = time.time()
        ok = self.navigate("/auth/login")
        if not ok:
            self._case("Login page loads", start, False,
                       error="Could not navigate to /auth/login")
            return
        visible = self.wait_for(".auth-form-card", timeout=6000)
        self._case("Login page shows auth form card", start, visible,
                   error="" if visible else ".auth-form-card not found")

    def _test_login_form_elements(self):
        self.assert_visible("Email input is visible", "input[formControlName='email']")
        self.assert_visible("Password input is visible", "input[formControlName='password']")
        self.assert_visible("Submit button is visible", ".btn-primary.auth-btn")

    def _test_invalid_login_shows_error(self):
        start = time.time()
        try:
            self.page.locator("input[formControlName='email']").fill("wrong@nobody.com")
            self.page.locator("input[formControlName='password']").fill("BadPass!999")
            self.page.locator(".btn-primary.auth-btn").click()
            # Wait for error message to appear
            self.page.wait_for_selector(".error-msg", state="visible", timeout=6000)
            passed = True
            self._case("Invalid login shows error message", start, passed)
        except Exception as e:
            self._case("Invalid login shows error message", start, False, error=str(e))

    def _test_register_page_loads(self):
        start = time.time()
        ok = self.navigate("/auth/register")
        if not ok:
            self._case("Register page loads", start, False,
                       error="Could not navigate to /auth/register")
            return
        visible = self.wait_for("input[formControlName='fullName']", timeout=6000)
        self._case("Register page shows full name input", start, visible,
                   error="" if visible else "Full name input not found")

    def _test_register_new_user(self):
        start = time.time()
        self._registered_email = f"ui_{uuid.uuid4().hex[:8]}@auto.test"
        try:
            self.navigate("/auth/register")
            self.page.wait_for_selector("input[formControlName='fullName']",
                                        state="visible", timeout=6000)
            self.page.locator("input[formControlName='fullName']").fill("UI Tester")
            self.page.locator("input[formControlName='email']").fill(self._registered_email)
            self.page.locator("input[formControlName='password']").fill(self._registered_password)
            self.page.locator("input[formControlName='confirmPassword']").fill(self._registered_password)
            self.page.locator(".btn-primary.auth-btn").click()
            # After successful register, should redirect (away from /auth/register)
            self.page.wait_for_timeout(2000)
            url = self.page.url
            passed = "/auth/register" not in url
            self._case(
                "Register new buyer account and redirect",
                start, passed,
                error="" if passed else f"Still on register page after submit: {url}",
            )
        except Exception as e:
            self._case("Register new buyer account and redirect", start, False, error=str(e))

    def _test_login_with_registered_user(self):
        start = time.time()
        if not self._registered_email:
            self._case("Login with registered account", start, False,
                       error="Registration failed; skipping login test")
            return
        try:
            self.navigate("/auth/login")
            self.page.wait_for_selector("input[formControlName='email']",
                                        state="visible", timeout=6000)
            self.page.locator("input[formControlName='email']").fill(self._registered_email)
            self.page.locator("input[formControlName='password']").fill(self._registered_password)
            self.page.locator(".btn-primary.auth-btn").click()
            self.page.wait_for_timeout(2000)
            url = self.page.url
            passed = "/auth/login" not in url
            self._case(
                "Login with registered account succeeds",
                start, passed,
                error="" if passed else f"Still on login page: {url}",
            )
        except Exception as e:
            self._case("Login with registered account succeeds", start, False, error=str(e))

    def _test_profile_link_visible_after_login(self):
        """After login the user chip / profile link must be visible in the header."""
        start = time.time()
        try:
            # Try user-chip first, then logout icon as a fallback signal
            visible = (
                self.wait_for(".user-chip", timeout=4000) or
                self.wait_for(".logout-mini", timeout=2000)
            )
            self._case(
                "User profile link visible in header after login",
                start, visible,
                error="" if visible else "Neither .user-chip nor .logout-mini found",
            )
        except Exception as e:
            self._case("User profile link visible in header after login",
                       start, False, error=str(e))

    def _test_logout(self):
        start = time.time()
        try:
            # Navigate to home so the storefront logout button is in the DOM
            self.navigate("/")
            self.page.wait_for_selector(".logout-mini", state="visible", timeout=6000)
            self.page.locator(".logout-mini").first.click()
            # auth.service.logout() redirects to /auth/login — wait for that URL
            self.page.wait_for_url("**/auth/login**", timeout=8000)
            passed = "/auth/login" in self.page.url
            self._case(
                "Logout redirects to /auth/login (session cleared)",
                start, passed,
                error="" if passed else f"Expected /auth/login, got {self.page.url}",
            )
        except Exception as e:
            self._case("Logout redirects to /auth/login", start, False, error=str(e))
