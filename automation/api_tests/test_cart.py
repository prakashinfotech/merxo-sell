"""Cart API tests - requires buyer authentication."""

import time
import uuid
import requests
from .base import ApiTestBase, TestSuiteResult, TestCase


class CartTests(ApiTestBase):
    def __init__(self, base_url: str, credentials: dict, timeout: int = 30):
        super().__init__(base_url, timeout)
        self.credentials = credentials
        self._cart_item_id: int | None = None

    def _login_buyer(self) -> bool:
        """Try config buyer first; fall back to registering a fresh test buyer."""
        creds = self.credentials.get("buyer", {})
        if self.authenticate(creds.get("email", ""), creds.get("password", "")):
            return True
        # Register a fresh buyer and login
        email = f"cart_test_{uuid.uuid4().hex[:8]}@auto.test"
        password = "AutoTest@123"
        try:
            self.session.post(
                f"{self.base_url}/auth/register",
                json={"fullName": "Cart Tester", "email": email,
                      "password": password, "confirmPassword": password},
                timeout=self.timeout,
            )
        except Exception:
            pass
        return self.authenticate(email, password)

    def run(self) -> TestSuiteResult:
        self._results.suite_name = "Cart API Tests"

        if not self._login_buyer():
            self._results.cases.append(TestCase(
                name="Cart tests - buyer login",
                passed=False,
                duration=0,
                error="Could not authenticate as buyer; skipping cart tests",
            ))
            return self._results

        self._test_get_cart()
        self._test_add_item()
        self._test_update_item()
        self._test_remove_item()
        self.clear_auth()
        self._test_cart_requires_auth()

        return self._results

    def _test_get_cart(self):
        t = time.time()
        r, err = self._request("GET", "/cart")
        self._assert("GET /cart - returns buyer cart", t, r, err, expected_status=200)

    def _test_add_item(self):
        t = time.time()
        r, err = self._request("POST", "/cart/items", json={
            "productId": 2,
            "quantity": 1,
        })
        passed = r is not None and r.status_code in (200, 201)
        tc = TestCase(
            name="POST /cart/items - add item",
            passed=passed,
            duration=time.time() - t,
            status_code=r.status_code if r else 0,
            error=err if r is None else ("" if passed else f"Got {r.status_code}"),
        )
        self._results.cases.append(tc)
        if passed and r:
            body = r.json()
            # Response: {cartId, items:[{cartItemId,...}]} or wrapped in data
            raw = body.get("data") or body
            items = (raw.get("items") if isinstance(raw, dict) else None) or []
            if items:
                self._cart_item_id = items[-1].get("cartItemId") or items[-1].get("id")

    def _test_update_item(self):
        if not self._cart_item_id:
            self._results.cases.append(TestCase(
                name="PUT /cart/items/{id} - update quantity",
                passed=False,
                duration=0,
                error="No cart item id available (add item failed)",
            ))
            return
        t = time.time()
        r, err = self._request("PUT", f"/cart/items/{self._cart_item_id}", json={"quantity": 3})
        self._assert(f"PUT /cart/items/{self._cart_item_id} - update quantity", t, r, err, expected_status=200)

    def _test_remove_item(self):
        if not self._cart_item_id:
            self._results.cases.append(TestCase(
                name="DELETE /cart/items/{id} - remove item",
                passed=False,
                duration=0,
                error="No cart item id available",
            ))
            return
        t = time.time()
        r, err = self._request("DELETE", f"/cart/items/{self._cart_item_id}")
        self._assert(f"DELETE /cart/items/{self._cart_item_id} - remove item", t, r, err, expected_status=200)

    def _test_cart_requires_auth(self):
        t = time.time()
        r, err = self._request("GET", "/cart")
        self._assert("GET /cart without token returns 401", t, r, err, expected_status=401)
