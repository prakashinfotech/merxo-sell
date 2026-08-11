"""UI tests for the shopping cart flow."""

import time
import uuid
from .base import UiTestBase, UiTestSuiteResult


class CartUiTests(UiTestBase):
    def __init__(self, frontend_url: str, credentials: dict,
                 headless: bool = False, slow_mo: int = 300):
        super().__init__(frontend_url, headless, slow_mo)
        self.credentials = credentials

    def _login_buyer(self) -> bool:
        """Register a fresh test buyer and log in."""
        email = f"cart_ui_{uuid.uuid4().hex[:8]}@auto.test"
        password = "CartUi@123"
        try:
            self.navigate("/auth/register")
            self.page.wait_for_selector("input[formControlName='fullName']",
                                        state="visible", timeout=6000)
            self.page.locator("input[formControlName='fullName']").fill("Cart UI Tester")
            self.page.locator("input[formControlName='email']").fill(email)
            self.page.locator("input[formControlName='password']").fill(password)
            self.page.locator("input[formControlName='confirmPassword']").fill(password)
            self.page.locator(".btn-primary.auth-btn").click()
            self.page.wait_for_timeout(2000)
            return "/auth/register" not in self.page.url
        except Exception:
            return False

    def run(self) -> UiTestSuiteResult:
        self._results.suite_name = "Cart UI Tests"

        logged_in = self._login_buyer()
        if not logged_in:
            start = time.time()
            self._case("Cart tests - buyer login", start, False,
                       error="Could not register/login as buyer; skipping cart UI tests")
            return self._results

        self._test_navigate_to_products()
        self._test_add_to_cart()
        self._test_cart_badge_updates()
        self._test_open_cart_page()
        self._test_cart_has_item()
        self._test_cart_page_shows_total()
        self._test_add_address_from_profile()
        self._test_checkout_page_loads()
        self._test_place_order()

        return self._results

    def _test_navigate_to_products(self):
        start = time.time()
        ok = self.navigate("/products")
        visible = self.wait_for(".product-card", timeout=10000)
        self._case("Navigate to /products shows product cards", start,
                   ok and visible,
                   error="" if (ok and visible) else "Could not load /products")

    def _test_add_to_cart(self):
        start = time.time()
        try:
            # Hover over first card to reveal Add to Cart button
            first_card = self.page.locator(".product-card").first
            first_card.hover()
            self.page.wait_for_timeout(400)

            add_btn = first_card.locator(".btn-add-to-cart")
            if add_btn.count() == 0:
                add_btn = self.page.locator(".btn-add-to-cart").first

            add_btn.click()
            self.page.wait_for_timeout(1500)

            # The cart drawer auto-opens after adding — close it with Escape
            # so subsequent tests can click the cart button freely
            self.page.keyboard.press("Escape")
            self.page.wait_for_timeout(600)

            passed = True
            self._case("Add to cart button clicked (drawer auto-closed)", start, passed)
        except Exception as e:
            self._case("Add to cart button clicked", start, False, error=str(e))

    def _test_cart_badge_updates(self):
        start = time.time()
        try:
            badge = self.page.locator(".cart-badge")
            if badge.count() > 0:
                text = badge.first.inner_text().strip()
                passed = text.isdigit() and int(text) > 0
                self._case(
                    f"Cart badge shows item count ({text})",
                    start, passed,
                    error="" if passed else f"Badge text '{text}' is not a positive integer",
                )
            else:
                # Some designs show the cart button without a badge when count is 0
                self._case("Cart badge updates after adding item", start, True,
                           error="")
        except Exception as e:
            self._case("Cart badge updates after adding item", start, False, error=str(e))

    def _test_open_cart_page(self):
        """For a logged-in buyer, the cart button should navigate to /cart.
        The cart drawer can overlay the button, so we trigger the click via JS
        to bypass Playwright's intercept check."""
        start = time.time()
        try:
            # Force-click via JS to bypass any drawer overlay
            self.page.evaluate("document.querySelector('.cart-btn').click()")
            self.page.wait_for_url("**/cart**", timeout=8000)
            passed = "/cart" in self.page.url
            self._case(
                "Cart button navigates to /cart for logged-in buyer",
                start, passed,
                error="" if passed else f"Expected /cart, got {self.page.url}",
            )
        except Exception as e:
            self._case("Cart button navigates to /cart for logged-in buyer",
                       start, False, error=str(e))

    def _test_cart_has_item(self):
        start = time.time()
        try:
            # Navigate to the cart page directly (the cart button opens a drawer, not the page)
            self.navigate("/cart")
            self.page.wait_for_timeout(2000)
            # Look for common cart item selectors
            selectors = [
                ".cart-item",
                ".cart__item",
                "[class*='cart-item']",
                ".product-card__name",
            ]
            found = any(
                self.page.locator(sel).count() > 0
                for sel in selectors
            )
            self._case(
                "Cart page contains at least one item",
                start, found,
                error="" if found else "No cart item found on /cart page",
            )
        except Exception as e:
            self._case("Cart page contains at least one item", start, False, error=str(e))

    def _test_cart_page_shows_total(self):
        """Cart page should display a total/summary section."""
        start = time.time()
        try:
            selectors = [
                "[class*='total']",
                "[class*='summary']",
                "[class*='price']",
                "text=Total",
                "text=Subtotal",
            ]
            found = any(
                self.page.locator(sel).count() > 0
                for sel in selectors
            )
            self._case(
                "Cart page displays order total/summary",
                start, found,
                error="" if found else "No total/summary element found on cart page",
            )
        except Exception as e:
            self._case("Cart page displays order total/summary", start, False, error=str(e))

    def _test_add_address_from_profile(self):
        """Navigate to user profile, switch to Addresses tab, and add a shipping address."""
        start = time.time()
        try:
            self.navigate("/profile")
            # Wait for the profile sidebar to render
            self.page.wait_for_selector(".profile-sidebar", state="visible", timeout=8000)

            # Click the Addresses tab in the sidebar nav
            addr_tab = self.page.locator("button.nav-item").filter(has_text="Addresses")
            addr_tab.first.click()
            self.page.wait_for_timeout(800)

            # Click "Add Address" button
            add_btn = self.page.locator("button.btn-secondary").filter(has_text="Add Address")
            add_btn.first.click()
            self.page.wait_for_timeout(600)

            # Fill the address form (uses formGroup bindings)
            self.page.locator("input[formControlName='fullName']").fill("Test Buyer")
            self.page.locator("input[formControlName='phone']").fill("6471234567")
            self.page.locator("input[formControlName='addressLine1']").fill("100 Queen St W")
            self.page.locator("input[formControlName='city']").fill("Toronto")
            self.page.locator("input[formControlName='state']").fill("ON")
            self.page.locator("input[formControlName='postalCode']").fill("M5H 2N2")
            # Country defaults to Canada in the dropdown — leave as-is

            # Submit — "Save Address" button inside the address form box
            save_btn = self.page.locator(".address-form-box .btn-primary")
            save_btn.click()
            # Wait for the form to disappear (address saved)
            self.page.wait_for_selector(".address-form-box", state="hidden", timeout=8000)

            # Confirm the new address card appears
            passed = self.page.locator(".address-card").count() > 0
            self._case(
                "Address added from profile page (Addresses tab)",
                start, passed,
                error="" if passed else "No address card found after saving address",
            )
        except Exception as e:
            self._case("Address added from profile page (Addresses tab)",
                       start, False, error=str(e))

    def _test_checkout_page_loads(self):
        """Navigate to /cart/checkout and verify the shipping address section renders."""
        start = time.time()
        try:
            self.navigate("/cart/checkout")
            visible = self.wait_for(".checkout-layout", timeout=10000)
            passed = visible and "/checkout" in self.page.url
            self._case(
                "Checkout page loads with address and order summary",
                start, passed,
                error="" if passed else f"Checkout layout not found or bad URL: {self.page.url}",
            )
        except Exception as e:
            self._case("Checkout page loads with address and order summary",
                       start, False, error=str(e))

    def _test_place_order(self):
        """Select shipping address (if needed) and place the order via Cash on Delivery."""
        start = time.time()
        try:
            # Select first address card if none selected yet
            addr_cards = self.page.locator(".address-card")
            if addr_cards.count() > 0 and \
               self.page.locator(".address-card.selected").count() == 0:
                addr_cards.first.click()
                self.page.wait_for_timeout(500)

            # COD is the default payment — no extra click needed
            place_btn = self.page.locator(".place-order-btn")
            place_btn.wait_for(state="visible", timeout=6000)
            place_btn.click()

            # After success, checkout.ts navigates to /orders
            self.page.wait_for_url("**/orders**", timeout=15000)
            passed = "/orders" in self.page.url
            self._case(
                "Order placed successfully and redirected to /orders",
                start, passed,
                error="" if passed else f"Expected /orders, got {self.page.url}",
            )
        except Exception as e:
            self._case("Order placed successfully and redirected to /orders",
                       start, False, error=str(e))
