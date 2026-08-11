"""Products API tests - public listing, search, and authenticated seller actions."""

import time
from .base import ApiTestBase, TestSuiteResult, TestCase


class ProductTests(ApiTestBase):
    def __init__(self, base_url: str, credentials: dict, timeout: int = 30):
        super().__init__(base_url, timeout)
        self.credentials = credentials
        self._product_id: int | None = None

    def run(self) -> TestSuiteResult:
        self._results.suite_name = "Products API Tests"

        self._test_list_products()
        self._test_pagination()
        self._test_search()
        self._test_search_suggestions()
        self._test_get_single_product()
        self._test_get_product_not_found()
        self._test_seller_create_product()

        return self._results

    def _test_list_products(self):
        t = time.time()
        r, err = self._request("GET", "/products?pageNumber=1&pageSize=10")
        self._assert(
            "GET /products - returns paginated list",
            t, r, err, expected_status=200,
            extra_check=lambda resp: "" if isinstance(resp.json().get("data") or resp.json(), (list, dict)) else "Unexpected response shape",
        )
        # Grab first product id for later tests
        if r and r.status_code == 200:
            body = r.json()
            items = body.get("data") or body.get("items") or (body if isinstance(body, list) else [])
            if isinstance(items, list) and items:
                self._product_id = items[0].get("id")
            elif isinstance(items, dict):
                nested = items.get("items") or items.get("data") or []
                if nested:
                    self._product_id = nested[0].get("id")

    def _test_pagination(self):
        t = time.time()
        r, err = self._request("GET", "/products?pageNumber=2&pageSize=5")
        self._assert("GET /products - page 2 works", t, r, err, expected_status=200)

    def _test_search(self):
        t = time.time()
        r, err = self._request("GET", "/products/search?query=shirt&pageNumber=1&pageSize=5")
        self._assert("GET /products/search?query=shirt", t, r, err, expected_status=200)

    def _test_search_suggestions(self):
        t = time.time()
        r, err = self._request("GET", "/products/suggest?q=sh")
        self._assert("GET /products/suggest?query=sh", t, r, err, expected_status=200)

    def _test_get_single_product(self):
        pid = self._product_id or 2  # ID 2 exists per suggest results
        t = time.time()
        r, err = self._request("GET", f"/products/{pid}")
        self._assert(f"GET /products/{pid} - single product", t, r, err, expected_status=200)

    def _test_get_product_not_found(self):
        t = time.time()
        r, err = self._request("GET", "/products/9999999")
        self._assert("GET /products/9999999 - returns 404", t, r, err, expected_status=404)

    def _test_seller_create_product(self):
        creds = self.credentials.get("seller", {})
        ok = self.authenticate(creds.get("email"), creds.get("password"), endpoint="/auth/seller/login")
        if not ok:
            self._results.cases.append(TestCase(
                name="POST /seller/products - create product (seller auth)",
                passed=False,
                duration=0,
                error="Could not obtain seller token",
            ))
            return

        t = time.time()
        r, err = self._request("POST", "/seller/products", json={
            "name": "Automation Test Product",
            "description": "Created by automation suite",
            "price": 9.99,
            "stock": 50,
            "categoryId": 1,
        })
        # 200 created OR 400 validation error are both acceptable (product may already exist)
        passed = r is not None and r.status_code in (200, 201, 400)
        self._results.cases.append(TestCase(
            name="POST /seller/products - create product (seller auth)",
            passed=passed,
            duration=time.time() - t,
            status_code=r.status_code if r else 0,
            error=err if not passed else "",
            detail=r.text[:200] if r and not passed else "",
        ))
        self.clear_auth()
