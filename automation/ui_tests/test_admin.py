"""UI tests for the admin dashboard."""

import time
from .base import UiTestBase, UiTestSuiteResult


class AdminUiTests(UiTestBase):
    def __init__(self, frontend_url: str, credentials: dict,
                 headless: bool = False, slow_mo: int = 300):
        super().__init__(frontend_url, headless, slow_mo)
        self.credentials = credentials

    def _admin_login(self) -> bool:
        creds = self.credentials.get("admin", {})
        try:
            self.navigate("/auth/admin/login")
            self.page.wait_for_selector("input[formControlName='email']",
                                        state="visible", timeout=6000)
            self.page.locator("input[formControlName='email']").fill(creds.get("email", ""))
            self.page.locator("input[formControlName='password']").fill(creds.get("password", ""))
            self.page.locator(".btn-primary.auth-btn").click()
            self.page.wait_for_timeout(2500)
            return "/auth" not in self.page.url
        except Exception:
            return False

    def run(self) -> UiTestSuiteResult:
        self._results.suite_name = "Admin UI Tests"

        start = time.time()
        logged_in = self._admin_login()
        if not logged_in:
            self._case("Admin UI - login", start, False,
                       error="Could not log in as admin; skipping admin UI tests")
            return self._results

        self._test_admin_dashboard_loads()
        self._test_stats_cards_visible()
        self._test_sidebar_navigation()
        self._test_products_section()
        self._test_orders_section()

        return self._results

    def _test_admin_dashboard_loads(self):
        start = time.time()
        ok = self.navigate("/admin")
        self.page.wait_for_timeout(1000)
        url = self.page.url
        passed = ok and "/admin" in url and "/auth" not in url
        self._case(
            "Admin dashboard page loads at /admin",
            start, passed,
            error="" if passed else f"Redirected away from /admin: {url}",
        )

    def _test_stats_cards_visible(self):
        """Dashboard stats cards (total orders, revenue, etc.) should be visible."""
        start = time.time()
        try:
            # Wait for any stat/metric card pattern
            selectors = [
                "[class*='stat']",
                "[class*='card']",
                "[class*='metric']",
                "[class*='dashboard']",
            ]
            found = False
            for sel in selectors:
                if self.wait_for(sel, timeout=3000):
                    count = self.page.locator(sel).count()
                    if count > 0:
                        found = True
                        break
            self._case(
                "Admin dashboard shows stat/metric cards",
                start, found,
                error="" if found else "No stat cards found on admin dashboard",
            )
        except Exception as e:
            self._case("Admin dashboard shows stat/metric cards", start, False, error=str(e))

    def _test_sidebar_navigation(self):
        """Admin sidebar/nav links must be present."""
        start = time.time()
        try:
            selectors = [
                "nav a", "aside a", "[class*='sidebar'] a",
                "[class*='nav'] a", "[class*='menu'] a",
            ]
            found = False
            for sel in selectors:
                links = self.page.locator(sel)
                if links.count() >= 2:
                    found = True
                    break
            self._case(
                "Admin sidebar has navigation links",
                start, found,
                error="" if found else "Could not find at least 2 nav links in admin area",
            )
        except Exception as e:
            self._case("Admin sidebar has navigation links", start, False, error=str(e))

    def _test_products_section(self):
        start = time.time()
        try:
            self.navigate("/admin/products")
            self.page.wait_for_timeout(1500)
            url = self.page.url
            # Should still be in /admin area
            in_admin = "/admin" in url and "/auth" not in url
            self._case(
                "Admin /admin/products page accessible",
                start, in_admin,
                error="" if in_admin else f"Redirected away: {url}",
            )
        except Exception as e:
            self._case("Admin /admin/products page accessible", start, False, error=str(e))

    def _test_orders_section(self):
        start = time.time()
        try:
            self.navigate("/admin/orders")
            self.page.wait_for_timeout(1500)
            url = self.page.url
            in_admin = "/admin" in url and "/auth" not in url
            self._case(
                "Admin /admin/orders page accessible",
                start, in_admin,
                error="" if in_admin else f"Redirected away: {url}",
            )
        except Exception as e:
            self._case("Admin /admin/orders page accessible", start, False, error=str(e))
