# Merxo E-Commerce Documentation Hub

Welcome to the official documentation directory for the **Merxo E-Commerce Platform** (Temu-Clone). This central directory contains complete, role-specific user manuals for platform shoppers, sellers, and administrators.

---

## 📚 User Manual Directory

Select the appropriate user manual based on your platform role:

| Role | User Manual Document | Quick Description |
| :--- | :--- | :--- |
| 🛒 **Customer / Buyer** | [**Customer User Manual**](USER_MANUAL.md) | Guide for account creation, product search, cart management, Razorpay & COD payments, coupon redemption, order tracking, and reviews. |
| 🏬 **Seller** | [**Seller Operations Manual**](SELLER_MANUAL.md) | Guide for seller registration, dashboard analytics, listing products & variants, inventory management, product moderation workflow, and order fulfillment. |
| 🛡️ **System Administrator** | [**Admin & Moderation Manual**](ADMIN_MANUAL.md) | Guide for executive dashboard KPIs, user/seller onboarding approvals, catalog & review moderation, category & banner management, coupons, and currency configuration. |

---

## 🚀 Quick Navigation & Key Features

### For Customers (Buyers)
- **Account & Profile**: [Register & Login](USER_MANUAL.md#1-account-management--security) \| [Address Book](USER_MANUAL.md#managing-profiles--delivery-addresses)
- **Shopping**: [Category Search & Filters](USER_MANUAL.md#2-browsing--discovering-products) \| [Currency Switcher](USER_MANUAL.md#currency-switcher)
- **Cart & Checkout**: [Coupons](USER_MANUAL.md#applying-coupon-codes) \| [Razorpay Payments](USER_MANUAL.md#5-checkout--secure-payments) \| [Order Tracking](USER_MANUAL.md#6-order-tracking--management)

### For Sellers
- **Store Setup**: [Registration & Profile](SELLER_MANUAL.md#1-seller-onboarding--registration) \| [Payout Setup](SELLER_MANUAL.md#payout--financial-settings)
- **Catalog Management**: [Add New Product](SELLER_MANUAL.md#creating-new-product-listings) \| [Variants (Color/Size)](SELLER_MANUAL.md#adding-product-variants-colors--sizes) \| [Moderation Flow](SELLER_MANUAL.md#4-product-moderation--approval-workflow)
- **Orders & Shipping**: [Order Processing](SELLER_MANUAL.md#5-order-processing--fulfillment) \| [Courier Tracking Numbers](SELLER_MANUAL.md#assigning-courier-tracking-numbers)

### For Administrators
- **Executive Overview**: [Platform Analytics & GMV](ADMIN_MANUAL.md#2-executive-admin-dashboard)
- **Approvals & Moderation**: [Seller Onboarding](ADMIN_MANUAL.md#4-seller-moderation--onboarding) \| [Product Queue](ADMIN_MANUAL.md#5-product-catalog--moderation-queue) \| [Review Moderation](ADMIN_MANUAL.md#10-review-moderation)
- **Platform Management**: [Categories & Banners](ADMIN_MANUAL.md#6-category--banner-management) \| [Coupons](ADMIN_MANUAL.md#7-coupons--promotions) \| [Currency Exchange](ADMIN_MANUAL.md#8-currency--exchange-rates)

---

## 🛠️ System Architecture Stack

- **Frontend**: Angular, RxJS, TypeScript, SCSS, HTML5
- **Backend**: ASP.NET Core Web API, C#, Entity Framework Core
- **Database**: SQL Server
- **Payments Integration**: Razorpay Payment Gateway (Cards, UPI, NetBanking, COD)
- **Authentication**: JWT Bearer Tokens with Role-Based Access Control (`Buyer`, `Seller`, `Admin`)

---

*For technical architecture or codebase contributions, refer to the root [README.md](../README.md) and [CONTRIBUTING.md](../CONTRIBUTING.md).*
