# System Administrator & Moderation User Manual

Welcome to the **Merxo E-Commerce Platform** Administrative Guide. This manual provides super-user instructions for platform control, seller onboarding, product moderation, financial oversight, promotional campaigns, and system settings.

---

## Table of Contents

1. [Administrator Authentication & Access Control](#1-administrator-authentication--access-control)
2. [Executive Admin Dashboard](#2-executive-admin-dashboard)
   - [Platform KPI Analytics](#platform-kpi-analytics)
   - [Revenue & Commission Overview](#revenue--commission-overview)
3. [User & Customer Management](#3-user--customer-management)
   - [User Directory & Role Assignments](#user-directory--role-assignments)
   - [Account Suspension & Reactivation](#account-suspension--reactivation)
4. [Seller Moderation & Onboarding](#4-seller-moderation--onboarding)
   - [Evaluating Seller Applications](#evaluating-seller-applications)
   - [Approval / Rejection Workflow](#approval--rejection-workflow)
5. [Product Catalog & Moderation Queue](#5-product-catalog--moderation-queue)
   - [Reviewing Submitted Listings](#reviewing-submitted-listings)
   - [Approving vs Rejecting Products](#approving-vs-rejecting-products)
   - [Takedown of Policy Violations](#takedown-of-policy-violations)
6. [Category & Banner Management](#6-category--banner-management)
   - [Managing Categories & Subcategories](#managing-categories--subcategories)
   - [Homepage Banner Campaigns](#homepage-banner-campaigns)
7. [Coupons & Promotions](#7-coupons--promotions)
   - [Creating Platform Coupon Codes](#creating-platform-coupon-codes)
   - [Usage Limits & History](#usage-limits--history)
8. [Currency & Exchange Rates](#8-currency--exchange-rates)
9. [Global Order Oversight & Payments](#9-global-order-oversight--payments)
   - [Platform Order Audit](#platform-order-audit)
   - [Razorpay Payment Reconciliation](#razorpay-payment-reconciliation)
10. [Review Moderation](#10-review-moderation)
11. [Platform Security & Best Practices](#11-platform-security--best-practices)

---

## 1. Administrator Authentication & Access Control

- **Admin Route**: `/admin` (Protected by `AdminGuard`).
- Access is restricted exclusively to authorized accounts with the `Admin` role.
- **Security Check**: Attempting to access `/admin` without admin privileges will automatically redirect to the login page or a 403 Forbidden page.

---

## 2. Executive Admin Dashboard

Navigate to `/admin/dashboard` to access real-time enterprise metrics:

```
┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐
│    Total GMV     │  │   Active Users   │  │  Active Sellers  │  │ Live Products    │
│  ₹ 1,245,000.00  │  │      4,820       │  │       312        │  │     14,500       │
└──────────────────┘  └──────────────────┘  └──────────────────┘  └──────────────────┘
```

### Platform KPI Analytics
- **Gross Merchandise Value (GMV)**: Total monetary value of orders processed across the entire marketplace.
- **Net Platform Revenue**: Total platform commission collected from seller sales.
- **User Growth Trends**: Daily/Monthly registration volume for Customers and Sellers.
- **System Health**: Active API services, database status, and order processing rates.

---

## 3. User & Customer Management

### User Directory & Role Assignments
1. Navigate to `/admin/users` or `/admin/customers`.
2. View a comprehensive user directory containing:
   - User ID, Name, Email, Phone, Role (`Buyer`, `Seller`, `Admin`), Registration Date, and Account Status (`Active` / `Disabled`).
3. **Filter**: Filter users by Role or Account Status.
4. **Edit Role**: Change a user's permission level (e.g., promote a User to Seller or Admin).

### Account Suspension & Reactivation
1. Search for the target user email or username.
2. Toggle the **Account Status** switch to **Disabled / Suspended**.
3. Suspended users will immediately lose access to place orders or log into the Seller/Admin portals.
4. To restore access, toggle status back to **Active**.

---

## 4. Seller Moderation & Onboarding

### Evaluating Seller Applications
1. Navigate to `/admin/sellers`.
2. Select the **Pending Applications** tab to view new seller requests.
3. Inspect seller application details:
   - Business Legal Name & GST/Tax Number
   - Contact Person Details
   - Physical Address & Warehouse Information
   - Payout Bank Details

### Approval / Rejection Workflow
- **To Approve**: Click **Approve Seller**. The seller status changes to `Active`, sending an automated welcome email with login authorization.
- **To Reject**: Click **Reject Application**. Enter a clear **Rejection Reason** (e.g., *Invalid business documentation*). The applicant is notified to re-apply with correct documentation.

---

## 5. Product Catalog & Moderation Queue

Maintain store-wide quality standards and prevent fraudulent or inappropriate listings.

### Reviewing Submitted Listings
1. Navigate to `/admin/moderation/products`.
2. The **Product Moderation Queue** lists all products submitted by sellers in `Pending Approval` status.
3. Click **Review** on any product to view images, descriptions, pricing, attributes, and seller information.

### Approving vs Rejecting Products
- **Approve Listing**: Click **Approve & Publish**. The product status updates to `Approved` and becomes immediately visible to customers on the site.
- **Reject Listing**: Click **Reject**. Select or type a specific rejection reason:
  - *Copyright / Trademark infringement*
  - *Inappropriate / Low-quality images*
  - *Incorrect category assignment*
  - *Prohibited item policy violation*
- The rejection note is delivered to the seller's portal for remediation.

### Takedown of Policy Violations
- To unpublish an active live product:
  1. Go to `/admin/products`.
  2. Search for the product by ID or title.
  3. Click **Unpublish / Delist**. The item is removed from search results.

---

## 6. Category & Banner Management

### Managing Categories & Subcategories
1. Navigate to `/admin/categories`.
2. **Add Main Category**: Click **+ Add Category**, enter Category Name, Slug, Icon Class, and Display Order.
3. **Add Subcategory**: Select a parent category, click **+ Add Subcategory**, and enter subcategory details.
4. **Edit / Delete**: Update category icons or delete obsolete categories.

### Homepage Banner Campaigns
1. Navigate to `/admin/banners`.
2. Click **+ Create Banner**.
3. Provide:
   - **Banner Title**: e.g., *Summer Electronics Super Sale*
   - **Image URL / Upload**: High-resolution wide hero banner image.
   - **Target Link URL**: Target category or product link (e.g., `/products?category=electronics`).
   - **Display Order**: Sequence on the home carousel (1, 2, 3...).
   - **Status**: Set to `Active` or `Inactive`.

---

## 7. Coupons & Promotions

### Creating Platform Coupon Codes
1. Navigate to `/admin/coupons`.
2. Click **+ Create Coupon**.
3. Fill in coupon parameters:
   - **Coupon Code**: Unique string (e.g., `SUMMER20`).
   - **Discount Type**: `Percentage` (e.g., 20%) or `Flat Amount` (e.g., ₹500 off).
   - **Discount Value**: Amount or percentage value.
   - **Minimum Order Amount**: Minimum cart value required to redeem coupon.
   - **Maximum Discount Limit**: Maximum cap for percentage-based discounts.
   - **Expiry Date**: Date and time when coupon expires.
   - **Total Usage Limit**: Maximum global redemptions across all users.

### Usage Limits & History
- Click **View Usage Logs** on any coupon to see which customers redeemed the code, associated order IDs, and total discount value disbursed.

---

## 8. Currency & Exchange Rates

1. Navigate to `/admin/currencies`.
2. **Supported Currencies**: View active currencies (e.g., INR, USD, EUR, GBP).
3. **Add Currency**: Enter Currency Code (e.g., `USD`), Symbol (`$`), and Exchange Rate relative to base currency.
4. **Update Rates**: Update exchange rates manually or enable automated rate syncing to ensure accurate checkout pricing across international currencies.

---

## 9. Global Order Oversight & Payments

### Platform Order Audit
1. Navigate to `/admin/orders`.
2. Search across all platform orders by **Order ID**, **Customer Name**, **Seller Name**, or **Date Range**.
3. View full item breakdowns, shipping addresses, fulfillment status, and payment logs.
4. **Status Override**: Administrative power to override order status in customer dispute scenarios.

### Razorpay Payment Reconciliation
- Go to `/admin/orders` or Razorpay Settings.
- Verify payment gateway transaction status against Razorpay Payment IDs (`pay_...`).
- Inspect payment statuses (`Captured`, `Failed`, `Refunded`).

---

## 10. Review Moderation

1. Navigate to `/admin/moderation/reviews` or `/admin/reviews`.
2. Inspect customer reviews submitted across all products.
3. Filter by **Flagged / Reported Reviews**.
4. Action Options:
   - **Approve**: Mark review as clean and published.
   - **Delete Review**: Remove reviews containing abusive language, spam, or fake content.

---

## 11. Platform Security & Best Practices

> [!CAUTION]
> Administrative actions such as deleting categories, disabling seller accounts, or overriding order statuses directly impact live business operations. Always verify records before executing permanent destructive commands.

- Perform periodic reviews of active admin user accounts under `/admin/users`.
- Ensure Razorpay API keys (`KeyId` and `KeySecret`) in `appsettings.json` are maintained securely and rotated periodically.

---

*Merxo E-Commerce Platform - System Administration Manual*
