"""UI tests for the products listing, search, and detail pages."""

import time
from .base import UiTestBase, UiTestSuiteResult


class ProductsUiTests(UiTestBase):
    def run(self) -> UiTestSuiteResult:
        self._results.suite_name = "Products UI Tests"

        self._test_products_page_loads()
        self._test_product_cards_visible()
        self._test_product_card_elements()
        self._test_search_from_header()
        self._test_search_shows_results()
        self._test_product_detail_page()
        self._test_back_navigation()

        return self._results

    def _test_products_page_loads(self):
        start = time.time()
        ok = self.navigate("/products")
        if not ok:
            self._case("Products page loads", start, False,
                       error="Could not navigate to /products")
            return
        visible = self.wait_for(".products-page", timeout=8000)
        self._case("Products page container visible", start, visible,
                   error="" if visible else ".products-page not found")

    def _test_product_cards_visible(self):
        start = time.time()
        try:
            self.page.wait_for_selector(".product-card", state="visible", timeout=10000)
            count = self.page.locator(".product-card").count()
            passed = count > 0
            self._case(
                f"Product cards rendered on /products ({count} cards)",
                start, passed,
                error="" if passed else "No .product-card elements found",
            )
        except Exception as e:
            self._case("Product cards rendered on /products", start, False, error=str(e))

    def _test_product_card_elements(self):
        """Each card must have an image, name, and price."""
        start = time.time()
        try:
            card = self.page.locator(".product-card").first
            has_name = card.locator(".product-card__name").count() > 0
            has_price = card.locator(".product-card__price").count() > 0
            has_image = card.locator(".product-card__image").count() > 0
            passed = has_name and has_price and has_image
            missing = []
            if not has_name:   missing.append("name")
            if not has_price:  missing.append("price")
            if not has_image:  missing.append("image")
            self._case(
                "Product card has name, price, and image",
                start, passed,
                error=f"Missing: {', '.join(missing)}" if missing else "",
            )
        except Exception as e:
            self._case("Product card has name, price, and image", start, False, error=str(e))

    def _test_search_from_header(self):
        """Type a query in the header search input and submit."""
        start = time.time()
        try:
            search = self.page.locator(".header-search__input")
            search.click()
            search.fill("shirt")
            self.page.locator(".header-search__btn").click()
            self.page.wait_for_timeout(1500)
            passed = True
            self._case("Search input accepts query and submits", start, passed)
        except Exception as e:
            self._case("Search input accepts query and submits", start, False, error=str(e))

    def _test_search_shows_results(self):
        """After searching 'shirt', at least one product card appears."""
        start = time.time()
        try:
            self.page.wait_for_selector(".product-card", state="visible", timeout=8000)
            count = self.page.locator(".product-card").count()
            passed = count > 0
            self._case(
                f"Search for 'shirt' shows results ({count} products)",
                start, passed,
                error="" if passed else "No results found after searching 'shirt'",
            )
        except Exception as e:
            self._case("Search for 'shirt' shows results", start, False, error=str(e))

    def _test_product_detail_page(self):
        """Click the first product card and verify the detail page loads."""
        start = time.time()
        try:
            self.navigate("/products")
            self.page.wait_for_selector(".product-card", state="visible", timeout=8000)
            # Click the name link inside the first card
            first_card = self.page.locator(".product-card").first
            first_card.click()
            self.page.wait_for_timeout(1500)
            # URL should change to /products/<id> or /products/<slug>
            url = self.page.url
            passed = "/products/" in url
            self._case(
                "Clicking product card opens detail page",
                start, passed,
                error="" if passed else f"URL did not change to detail: {url}",
            )
        except Exception as e:
            self._case("Clicking product card opens detail page", start, False, error=str(e))

    def _test_back_navigation(self):
        """Browser back button returns to the products list."""
        start = time.time()
        try:
            self.page.go_back()
            self.page.wait_for_timeout(1000)
            self.page.wait_for_selector(".product-card", state="visible", timeout=8000)
            passed = True
            self._case("Browser back returns to products list", start, passed)
        except Exception as e:
            self._case("Browser back returns to products list", start, False, error=str(e))
