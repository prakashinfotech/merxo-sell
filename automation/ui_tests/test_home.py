"""UI tests for the home page."""

import time
from .base import UiTestBase, UiTestSuiteResult


class HomeUiTests(UiTestBase):
    def run(self) -> UiTestSuiteResult:
        self._results.suite_name = "Home Page UI Tests"

        if not self.navigate("/"):
            self._case("Home page loads", time.time(), False,
                       error="Could not navigate to frontend URL")
            return self._results

        self._test_page_loads()
        self._test_header_logo()
        self._test_search_bar()
        self._test_cart_button()
        self._test_auth_buttons()
        self._test_hero_banner()
        self._test_product_cards()
        self._test_404_handling()

        return self._results

    def _test_page_loads(self):
        start = time.time()
        try:
            title = self.page.title()
            passed = bool(title)
            self._case("Home page loads and has title", start, passed,
                       error="" if passed else "Page title is empty")
        except Exception as e:
            self._case("Home page loads and has title", start, False, error=str(e))

    def _test_header_logo(self):
        self.assert_visible("Header logo is visible", ".logo")

    def _test_search_bar(self):
        self.assert_visible("Search input is visible", ".header-search__input")

    def _test_cart_button(self):
        self.assert_visible("Cart button is visible", ".cart-btn")

    def _test_auth_buttons(self):
        self.assert_visible("Login button is visible", ".header-btn-ghost")
        self.assert_visible("Register button is visible", ".header-btn-lime")

    def _test_hero_banner(self):
        self.assert_visible("Hero banner section is visible", ".offer-hero-section", timeout=8000)

    def _test_product_cards(self):
        start = time.time()
        try:
            # Wait for at least one product card to appear
            self.page.wait_for_selector(".product-card", state="visible", timeout=10000)
            count = self.page.locator(".product-card").count()
            passed = count > 0
            self._case(
                f"Product cards visible on home ({count} found)",
                start, passed,
                error="" if passed else "No .product-card elements found",
            )
        except Exception as e:
            self._case("Product cards visible on home", start, False, error=str(e))

    def _test_404_handling(self):
        start = time.time()
        navigated = self.navigate("/this-route-does-not-exist-xyz")
        self.page.wait_for_timeout(1000)
        url = self.page.url
        # Angular should show the 404 page or redirect
        passed = navigated and ("404" in url or "not-found" in url or
                                self.page.locator("text=404").count() > 0 or
                                self.page.locator("text=Page Not Found").count() > 0 or
                                self.page.locator("text=not found").count() > 0)
        self._case("Unknown route shows 404 page", start, passed,
                   error="" if passed else f"Expected 404 page, got: {url}")
        # Navigate back home for next suites
        self.navigate("/")
