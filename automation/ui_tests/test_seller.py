"""UI tests for the seller portal."""

import time
from .base import UiTestBase, UiTestSuiteResult


class SellerUiTests(UiTestBase):
    def __init__(self, frontend_url: str, credentials: dict,
                 headless: bool = False, slow_mo: int = 600):
        super().__init__(frontend_url, headless, slow_mo)
        self.credentials = credentials

    def _seller_login(self) -> bool:
        creds = self.credentials.get("seller", {})
        try:
            self.navigate("/auth/seller/login")
            self.page.wait_for_selector("input[formControlName='email']",
                                        state="visible", timeout=6000)
            self.page.locator("input[formControlName='email']").fill(creds.get("email", ""))
            self.page.locator("input[formControlName='password']").fill(creds.get("password", ""))
            self.page.locator(".btn-primary.auth-btn").click()
            # Wait for redirect away from auth pages
            self.page.wait_for_url("**/seller/**", timeout=10000)
            return "/seller/" in self.page.url
        except Exception:
            return False

    def run(self) -> UiTestSuiteResult:
        self._results.suite_name = "Seller UI Tests"

        start = time.time()
        logged_in = self._seller_login()
        if not logged_in:
            self._case("Seller UI - login", start, False,
                       error="Could not log in as seller; skipping seller UI tests")
            return self._results

        self._case("Seller login redirects to /seller/dashboard", start, True)

        self._test_seller_dashboard_loads()
        self._test_seller_stats_cards()
        self._test_seller_sidebar_links()
        self._test_seller_products_page()
        self._test_seller_orders_page()
        self._test_seller_profile_page()
        self._test_seller_logout()

        return self._results

    def _test_seller_dashboard_loads(self):
        start = time.time()
        try:
            url = self.page.url
            in_dashboard = "/seller" in url and "/auth" not in url
            # Also check a heading that confirms we're on the dashboard
            heading_visible = self.wait_for(".welcome-title", timeout=5000)
            passed = in_dashboard and heading_visible
            self._case(
                "Seller dashboard page loads at /seller/dashboard",
                start, passed,
                error="" if passed else f"Unexpected URL or missing heading: {url}",
            )
        except Exception as e:
            self._case("Seller dashboard page loads at /seller/dashboard",
                       start, False, error=str(e))

    def _test_seller_stats_cards(self):
        start = time.time()
        try:
            selectors = [
                ".stat-card",
                ".kpi-card",
                "[class*='stat']",
                "[class*='metric']",
            ]
            found = False
            for sel in selectors:
                if self.wait_for(sel, timeout=4000):
                    if self.page.locator(sel).count() > 0:
                        found = True
                        break
            self._case(
                "Seller dashboard shows stat/KPI cards",
                start, found,
                error="" if found else "No stat or KPI cards found on seller dashboard",
            )
        except Exception as e:
            self._case("Seller dashboard shows stat/KPI cards", start, False, error=str(e))

    def _test_seller_sidebar_links(self):
        start = time.time()
        try:
            # Seller shell has <aside class="seller-sidebar"> with <nav class="sidebar-nav"> <a> links
            links = self.page.locator(".seller-sidebar .sidebar-nav a")
            count = links.count()
            passed = count >= 3  # dashboard, products, orders, reviews, profile
            self._case(
                f"Seller sidebar has navigation links ({count} found)",
                start, passed,
                error="" if passed else f"Expected >= 3 sidebar links, found {count}",
            )
        except Exception as e:
            self._case("Seller sidebar has navigation links", start, False, error=str(e))

    def _test_seller_products_page(self):
        start = time.time()
        try:
            self.navigate("/seller/products")
            self.page.wait_for_timeout(1500)
            url = self.page.url
            in_seller = "/seller" in url and "/auth" not in url
            self._case(
                "Seller /seller/products page accessible",
                start, in_seller,
                error="" if in_seller else f"Redirected away from seller products: {url}",
            )
        except Exception as e:
            self._case("Seller /seller/products page accessible", start, False, error=str(e))

    def _test_seller_orders_page(self):
        start = time.time()
        try:
            self.navigate("/seller/orders")
            self.page.wait_for_timeout(1500)
            url = self.page.url
            in_seller = "/seller" in url and "/auth" not in url
            self._case(
                "Seller /seller/orders page accessible",
                start, in_seller,
                error="" if in_seller else f"Redirected away from seller orders: {url}",
            )
        except Exception as e:
            self._case("Seller /seller/orders page accessible", start, False, error=str(e))

    def _test_seller_profile_page(self):
        start = time.time()
        try:
            self.navigate("/seller/profile")
            self.page.wait_for_timeout(1500)
            url = self.page.url
            in_seller = "/seller" in url and "/auth" not in url
            self._case(
                "Seller /seller/profile page accessible",
                start, in_seller,
                error="" if in_seller else f"Redirected away from seller profile: {url}",
            )
        except Exception as e:
            self._case("Seller /seller/profile page accessible", start, False, error=str(e))

    def _test_seller_logout(self):
        start = time.time()
        try:
            self.navigate("/seller/dashboard")
            self.page.wait_for_selector(".btn-logout-sidebar", state="visible", timeout=6000)
            self.page.locator(".btn-logout-sidebar").click()
            self.page.wait_for_url("**/auth/**", timeout=8000)
            passed = "/auth" in self.page.url
            self._case(
                "Seller logout redirects to auth page",
                start, passed,
                error="" if passed else f"Expected /auth/*, got {self.page.url}",
            )
        except Exception as e:
            self._case("Seller logout redirects to auth page", start, False, error=str(e))
