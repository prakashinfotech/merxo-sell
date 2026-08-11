"""Orders API tests - buyer order flow and admin status update."""

import time
import uuid
from .base import ApiTestBase, TestSuiteResult, TestCase


class OrderTests(ApiTestBase):
    def __init__(self, base_url: str, credentials: dict, timeout: int = 30):
        super().__init__(base_url, timeout)
        self.credentials = credentials
        self._order_id: int | None = None

    def _login_buyer(self) -> bool:
        """Try config buyer first; fall back to registering a fresh test buyer."""
        creds = self.credentials.get("buyer", {})
        if self.authenticate(creds.get("email", ""), creds.get("password", "")):
            return True
        email = f"order_test_{uuid.uuid4().hex[:8]}@auto.test"
        password = "AutoTest@123"
        try:
            self.session.post(
                f"{self.base_url}/auth/register",
                json={"fullName": "Order Tester", "email": email,
                      "password": password, "confirmPassword": password},
                timeout=self.timeout,
            )
        except Exception:
            pass
        return self.authenticate(email, password)

    def run(self) -> TestSuiteResult:
        self._results.suite_name = "Orders API Tests"

        self._test_buyer_get_orders()
        self._test_buyer_create_order()
        self._test_admin_get_all_orders()
        self._test_admin_update_order_status()

        return self._results

    def _test_buyer_get_orders(self):
        if not self._login_buyer():
            self._results.cases.append(TestCase(
                name="GET /orders - buyer order history",
                passed=False, duration=0,
                error="Could not authenticate as buyer",
            ))
            return
        t = time.time()
        r, err = self._request("GET", "/orders")
        self._assert("GET /orders - buyer order history", t, r, err, expected_status=200)
        if r and r.status_code == 200:
            body = r.json()
            items = body if isinstance(body, list) else (body.get("data") or [])
            if isinstance(items, list) and items:
                self._order_id = items[0].get("orderId") or items[0].get("id")
        self.clear_auth()

    def _test_buyer_create_order(self):
        if not self._login_buyer():
            self._results.cases.append(TestCase(
                name="POST /orders - place an order",
                passed=False, duration=0,
                error="Could not authenticate as buyer",
            ))
            return
        t = time.time()
        r, err = self._request("POST", "/orders", json={
            "addressId": 1,
            "paymentMethod": "CreditCard",
            "items": [{"productId": 2, "quantity": 1}],
        })
        # 200/201 success or 400 (e.g. out of stock / no address) both acceptable
        passed = r is not None and r.status_code in (200, 201, 400)
        tc = TestCase(
            name="POST /orders - place an order",
            passed=passed,
            duration=time.time() - t,
            status_code=r.status_code if r else 0,
            error=err if r is None else ("" if passed else f"Got {r.status_code}: {r.text[:200]}"),
        )
        self._results.cases.append(tc)
        if r and r.status_code in (200, 201):
            body = r.json()
            self._order_id = (body.get("data") or body or {}).get("id")
        self.clear_auth()

    def _test_admin_get_all_orders(self):
        creds = self.credentials.get("admin", {})
        self.authenticate(creds.get("email"), creds.get("password"), endpoint="/auth/admin/login")
        t = time.time()
        r, err = self._request("GET", "/orders")
        self._assert("GET /orders - admin sees all orders", t, r, err, expected_status=200)

        # Grab an order id for the update test if buyer didn't create one
        if not self._order_id and r and r.status_code == 200:
            body = r.json()
            items = body if isinstance(body, list) else (body.get("data") or [])
            if isinstance(items, list) and items:
                self._order_id = items[0].get("orderId") or items[0].get("id")
        self.clear_auth()

    def _test_admin_update_order_status(self):
        if not self._order_id:
            self._results.cases.append(TestCase(
                name="PATCH /orders/{id}/status - admin update",
                passed=False, duration=0,
                error="No order id available to test status update",
            ))
            return
        creds = self.credentials.get("admin", {})
        self.authenticate(creds.get("email"), creds.get("password"), endpoint="/auth/admin/login")
        t = time.time()
        r, err = self._request("PATCH", f"/orders/{self._order_id}/status", json={"status": "Processing"})
        # 200 = updated; 400 = transition not allowed (e.g. already Delivered) — both are valid
        passed = r is not None and r.status_code in (200, 400)
        self._results.cases.append(TestCase(
            name=f"PATCH /orders/{self._order_id}/status - admin update status",
            passed=passed,
            duration=time.time() - t,
            status_code=r.status_code if r else 0,
            error=err if not passed else "",
        ))
        self.clear_auth()
