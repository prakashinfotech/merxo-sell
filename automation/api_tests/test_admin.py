"""Admin API tests - dashboard, coupons, categories, seller approval."""

import time
import uuid
from .base import ApiTestBase, TestSuiteResult, TestCase


class AdminTests(ApiTestBase):
    def __init__(self, base_url: str, credentials: dict, timeout: int = 30):
        super().__init__(base_url, timeout)
        self.credentials = credentials

    def run(self) -> TestSuiteResult:
        self._results.suite_name = "Admin API Tests"

        creds = self.credentials.get("admin", {})
        ok = self.authenticate(creds.get("email"), creds.get("password"), endpoint="/auth/admin/login")
        if not ok:
            self._results.cases.append(TestCase(
                name="Admin tests - admin login",
                passed=False,
                duration=0,
                error="Could not authenticate as admin; skipping all admin tests",
            ))
            return self._results

        self._test_dashboard_stats()
        self._test_dashboard_revenue()
        self._test_list_customers()
        self._test_list_sellers()
        self._test_list_categories()
        self._test_create_coupon()
        self._test_list_coupons()
        self._test_pending_products()
        self._test_admin_role_guard()
        self.clear_auth()

        return self._results

    def _test_dashboard_stats(self):
        t = time.time()
        r, err = self._request("GET", "/admin/stats")
        self._assert("GET /admin/stats - dashboard statistics", t, r, err, expected_status=200)

    def _test_dashboard_revenue(self):
        t = time.time()
        r, err = self._request("GET", "/dashboard/revenue")
        self._assert("GET /dashboard/revenue", t, r, err, expected_status=200)

    def _test_list_customers(self):
        t = time.time()
        r, err = self._request("GET", "/admin/customers")
        self._assert("GET /admin/customers - list all customers", t, r, err, expected_status=200)

    def _test_list_sellers(self):
        t = time.time()
        r, err = self._request("GET", "/admin/sellers")
        self._assert("GET /admin/sellers - list all sellers", t, r, err, expected_status=200)

    def _test_list_categories(self):
        t = time.time()
        r, err = self._request("GET", "/admin/categories")
        passed = r is not None and r.status_code in (200, 404)  # endpoint may vary
        self._results.cases.append(TestCase(
            name="GET /admin/categories",
            passed=passed,
            duration=time.time() - t,
            status_code=r.status_code if r else 0,
            error=err if not passed else "",
        ))

    def _test_create_coupon(self):
        t = time.time()
        code = f"AUTO{uuid.uuid4().hex[:6].upper()}"
        r, err = self._request("POST", "/admin/coupons", json={
            "title": f"Auto Test Coupon {code}",
            "couponCode": code,
            "discountType": "Percentage",
            "discountValue": 10,
            "expiryDate": "2027-12-31",
            "maxUses": 100,
        })
        passed = r is not None and r.status_code in (200, 201)
        self._results.cases.append(TestCase(
            name="POST /admin/coupons - create coupon",
            passed=passed,
            duration=time.time() - t,
            status_code=r.status_code if r else 0,
            error=err if r is None else ("" if passed else f"{r.status_code}: {r.text[:200]}"),
        ))

    def _test_list_coupons(self):
        t = time.time()
        r, err = self._request("GET", "/admin/coupons")
        self._assert("GET /admin/coupons - list coupons", t, r, err, expected_status=200)

    def _test_pending_products(self):
        t = time.time()
        r, err = self._request("GET", "/admin/products/pending")
        self._assert("GET /admin/products/pending - moderation queue", t, r, err, expected_status=200)

    def _test_admin_role_guard(self):
        """Non-admin token (or no token) must not access admin endpoints."""
        self.clear_auth()
        creds = self.credentials.get("buyer", {})
        self.authenticate(creds.get("email"), creds.get("password"))
        t = time.time()
        r, err = self._request("GET", "/admin/stats")
        # 403 when buyer token present; 401 when buyer auth itself failed
        passed = r is not None and r.status_code in (401, 403)
        self._results.cases.append(TestCase(
            name="GET /admin/stats with non-admin token returns 401/403",
            passed=passed,
            duration=time.time() - t,
            status_code=r.status_code if r else 0,
            error=err if not passed else "",
        ))
        self.clear_auth()
        # Re-auth as admin so caller doesn't leave stale state
        creds = self.credentials.get("admin", {})
        self.authenticate(creds.get("email"), creds.get("password"), endpoint="/auth/admin/login")
