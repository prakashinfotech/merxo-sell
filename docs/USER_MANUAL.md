# 🛍️ Merxo E-Commerce — Customer User Guide

> **Welcome to the Merxo E-Commerce Platform!**  
> This user guide is designed to provide an interactive, visual walkthrough for discovering products, placing orders, completing payments, tracking shipments, and leaving reviews.

---

## 🎯 Quick Navigation & Role Overview

| Attribute | Details |
| :--- | :--- |
| **User Role** | Customer / Buyer |
| **Access URL** | `https://merxo.com` (or local `http://localhost:4200`) |
| **Supported Devices** | Desktop Web, Tablet, Mobile Browser |
| **Payment Gateway** | Razorpay (UPI, Credit/Debit Cards, NetBanking) & Cash on Delivery (COD) |
| **Supported Currencies** | INR (₹), USD ($), EUR (€), GBP (£) |

---

## 🗺️ Customer Journey Overview

```mermaid
flowchart LR
    A[🔍 Search / Browse Catalog] --> B[👕 Select Product & Variant]
    B --> C[🛒 Add to Shopping Cart]
    C --> D[🎟️ Apply Coupon Code]
    D --> E[💳 Secure Checkout]
    E -->|Razorpay / COD| F[📦 Order Placed]
    F --> G[🚚 Track Shipment Live]
    G --> H[⭐ Rate & Review Product]
```

---

## 📌 Table of Contents

1. [Account Setup & Profile Security](#1-account-setup--profile-security)
2. [Product Discovery & Shopping Experience](#2-product-discovery--shopping-experience)
3. [Product Detail Page (PDP) & Variants](#3-product-detail-page-pdp--variants)
4. [Shopping Cart & Coupon Management](#4-shopping-cart--coupon-management)
5. [Checkout & Secure Payment Flow](#5-checkout--secure-payment-flow)
6. [Order Tracking & Post-Purchase Services](#6-order-tracking--post-purchase-services)
7. [Submitting Product Ratings & Reviews](#7-submitting-product-ratings--reviews)
8. [Customer Support & FAQ](#8-customer-support--faq)

---

## 1. Account Setup & Profile Security

### 📱 Interface Wireframe: Header & User Controls
```
┌─────────────────────────────────────────────────────────────────────────────────────────────────┐
│ 🛍️ MERXO    [ 🔍 Search products, brands, categories...     ] 🔍   [🌐 USD $ ▾]  [🛒 Cart (3)]  [👤 Account ▾] │
├─────────────────────────────────────────────────────────────────────────────────────────────────┤
│  Electronics  │  Fashion  │  Home & Living  │  Beauty  │  Flash Deals ⚡  │  Coupons 🎟️             │
└─────────────────────────────────────────────────────────────────────────────────────────────────┘
```

### Step-by-Step Registration & Login

> [!TIP]
> **Creating an account unlocks faster 1-click checkout, saved address books, wishlists, and live SMS/Email order updates.**

1. **Create Account**:
   - Click **Sign In / Register** in the top navigation header.
   - Click **Create Customer Account**.
   - Fill in your **Full Name**, **Email Address**, **Mobile Number**, and **Password**.
   - Click **Create Account**.

2. **Managing Delivery Addresses**:
   - Go to **My Account** > **Address Book**.
   - Click **+ Add New Address**.
   - Enter your Street, Apartment/Suite, City, State, Postal Code, and Country.
   - Toggle **Set as Default Shipping Address** for instant 1-click checkout.

3. **Account Recovery**:
   - Forgot your password? Click **Forgot Password?** on the sign-in modal.
   - Check your email inbox for a secure verification link to reset your credentials.

---

## 2. Product Discovery & Shopping Experience

### 🔍 Search, Filter & Sorting Layout

```
┌───────────────────────────┬────────────────────────────────────────────────────────┐
│ ⚙️ FILTERS                │  Catalog Products (Showing 1 - 24 of 120)  [Sort By: Popularity ▾]│
├───────────────────────────┼────────────────────────────────────────────────────────┤
│ Category                  │  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐ │
│  ☑️ Electronics (45)      │  │ 🖼️ Product Image │  │ 🖼️ Product Image │  │ 🖼️ Product Image │ │
│  ☐ Fashion (32)           │  │ Wireless Headset │  │ Smart Watch V2   │  │ Leather Backpack │ │
│                           │  │ ⭐⭐⭐⭐⭐ (4.8)    │  │ ⭐⭐⭐⭐☆ (4.2)    │  │ ⭐⭐⭐⭐⭐ (5.0)    │ │
│ Price Range               │  │ $49.99  ~~$69.99~~ │  │ $89.00           │  │ $35.50           │ │
│  [ $ 10 ] ─── [ $ 200 ]   │  │ [🛒 Add to Cart] │  │ [🛒 Add to Cart] │  │ [🛒 Add to Cart] │ │
│                           │  └──────────────────┘  └──────────────────┘  └──────────────────┘ │
│ Rating Filter             │                                                        │
│  🔴 ⭐ 4.0 & Above        │                                                        │
└───────────────────────────┴────────────────────────────────────────────────────────┘
```

### How to Find Products Quickly

* **Global Search Bar**: Type keywords like `"Wireless Headphones"`, `"Nike"`, or SKU numbers into the top search bar.
* **Category Menu**: Browse hierarchical categories and subcategories from the top header navigation.
* **Currency Switcher**:
  - Click the **Currency Selector** dropdown (`USD $`, `INR ₹`, `EUR €`, `GBP £`) in the top right.
  - All catalog prices, discounts, taxes, and shipping rates immediately adjust using real-time conversion rates.
* **Flash Sales & Banners**: Click hero promotional banners on the homepage to open curated discount collections.

---

## 3. Product Detail Page (PDP) & Variants

### 🖼️ PDP Interactive Layout

```
┌──────────────────────────────────────────────┬──────────────────────────────────────────────────┐
│                                              │ 🎧 Wireless Noise-Cancelling Headphones          │
│   ┌──────────────────────────────────────┐   │ Brand: AudioTech | ⭐ 4.9 (128 Customer Reviews)  │
│   │                                      │   │ ──────────────────────────────────────────────── │
│   │                                      │   │ Price: $129.99  ~~$159.99~~  [ 20% OFF ]        │
│   │        PRIMARY PRODUCT IMAGE         │   │ Stock Status: 🟢 In Stock (Only 4 items left!)    │
│   │                                      │   │ ──────────────────────────────────────────────── │
│   │                                      │   │ Select Color:  [⚫ Black] [⚪ Silver] [🔵 Navy]   │
│   └──────────────────────────────────────┘   │ Select Size:   [ M ]  [ L ]                      │
│   [📷 Thumbnail 1] [📷 Thumbnail 2] [📷 Thumb 3]│ ──────────────────────────────────────────────── │
│                                              │ Quantity: [ - ]  1  [ + ]                        │
│                                              │ [ 🛒 ADD TO CART ]     [ ⚡ BUY NOW ]            │
│                                              │ [ ❤️ Add to Wishlist ]                           │
└───────────────────────────┴──────────────────────────────────────────────────┘
```

### Choosing Variants & Inspecting Stock
1. **Color Selection**: Click a color swatch to update product images and variant-specific pricing.
2. **Size Selection**: Select your size (e.g., *Small, Medium, Large*).
3. **Stock Alerts**:
   - 🟢 **In Stock**: Available for immediate dispatch.
   - 🟡 **Low Stock (< 5 items)**: High demand item; order soon.
   - 🔴 **Out of Stock**: Item unavailable; add to wishlist to receive back-in-stock alerts.

---

## 4. Shopping Cart & Coupon Management

```
┌──────────────────────────────────────────────────────────┬──────────────────────────────────────┐
│ Shopping Cart (2 Items)                                  │ 🧾 Order Summary                     │
├──────────────────────────────────────────────────────────┼──────────────────────────────────────┤
│ 📦 Wireless Headphones (Color: Black, Size: M)           │ Subtotal:                   $129.99 │
│    Price: $129.99  |  Qty: [ - ] 1 [ + ]                 │ Coupon Discount (SAVE20):   -$20.00 │
│    [🗑️ Remove]                                           │ Estimated Shipping:           $5.00 │
│                                                          │ Estimated Tax (5%):           $5.50 │
│ 🎟️ HAVE A COUPON CODE?                                   │ ──────────────────────────────────── │
│    [ SAVE20               ]  [ 🎟️ APPLY COUPON ]         │ Total Amount:               $120.49 │
│    ✅ Coupon 'SAVE20' Applied ($20.00 Off)               │                                      │
│                                                          │ [ 🔒 PROCEED TO CHECKOUT ]           │
└──────────────────────────────────────────────────────────┴──────────────────────────────────────┘
```

### How to Apply Promotional Coupons
1. Go to your **Shopping Cart** page.
2. Type your promo code (e.g., `WELCOME10` or `SUMMER20`) into the **Have a Coupon?** input field.
3. Click **Apply Coupon**.
4. The discount amount will be subtracted from your order subtotal automatically.

---

## 5. Checkout & Secure Payment Flow

### Step-by-Step Checkout

```
[ 1. Select Address ] ───> [ 2. Select Payment Method ] ───> [ 3. Review & Place Order ]
```

> [!IMPORTANT]
> Merxo utilizes **Razorpay Payment Gateway** with 256-bit SSL encryption to ensure 100% secure payment transactions.

#### Available Payment Methods
- 💳 **Credit / Debit Cards**: Visa, Mastercard, RuPay, American Express.
- 📱 **UPI Payments**: Google Pay, PhonePe, Paytm, BHIM UPI ID.
- 🏦 **NetBanking**: Direct payment from 50+ supported major banks.
- 💵 **Cash on Delivery (COD)**: Pay cash directly to the delivery agent upon receiving your package.

#### Completing Payment via Razorpay
1. Select **Razorpay Secure Payment**.
2. Click **Place Order & Pay**.
3. The secure Razorpay modal opens:
   - Select **UPI / Card / NetBanking**.
   - Enter OTP / Bank authentication.
4. Upon successful payment verification, you will be automatically redirected to the **Order Confirmation Screen**.

---

## 6. Order Tracking & Post-Purchase Services

### 🚚 Order Status Lifecycle

```
┌──────────────┐      ┌──────────────┐      ┌──────────────┐      ┌──────────────┐      ┌──────────────┐
│  🟡 Pending  │ ───> │ 🔵 Processing│ ───> │  🚚 Shipped  │ ───> │ 📦 Out for   │ ───> │ ✅ Delivered │
│ (Order Recd) │      │ (Pack & Prep)│      │(In Transit)  │      │  Delivery    │      │ (Completed)  │
└──────────────┘      └──────────────┘      └──────────────┘      └──────────────┘      └──────────────┘
```

| Order Status Badge | Meaning & Next Step |
| :--- | :--- |
| 🟡 **Pending** | Order received; awaiting seller confirmation or payment capture. |
| 🔵 **Processing** | Seller is packing items and assigning package courier. |
| 🚚 **Shipped** | Package handed to courier; tracking number assigned. |
| 📦 **Out for Delivery** | Courier agent is delivering package to your doorstep today. |
| ✅ **Delivered** | Order delivered successfully. You can now leave a product review! |
| 🔴 **Cancelled** | Order cancelled. Prepaid orders are refunded within 5-7 business days. |

### How to Track & Manage Orders
1. Navigate to **My Account** > **Orders** (`/orders`).
2. Locate your Order ID (e.g., `#ORD-98421`).
3. Click **Track Package** to view live courier status updates and AWB tracking numbers.
4. **Download Invoice**: Click **📄 Download PDF Invoice** for accounting/warranty records.
5. **Cancel Order**: Click **Cancel Order** (Available only while status is *Pending* or *Processing*).

---

## 7. Submitting Product Ratings & Reviews

> [!NOTE]
> Only verified buyers who have received a **Delivered** order can submit product reviews.

```
┌─────────────────────────────────────────────────────────────────────────────────────────────────┐
│ ✍️ Write a Customer Review for: Wireless Noise-Cancelling Headphones                            │
├─────────────────────────────────────────────────────────────────────────────────────────────────┤
│ Overall Rating:   ⭐⭐⭐⭐⭐ (5 / 5 Stars)                                                         │
│ Review Title:     [ Outstanding battery life and incredible sound quality!                    ] │
│ Detailed Review:  [ I purchased these headphones two weeks ago. Battery lasts over 30 hours... ] │
│                   [                                                                           ] │
│                                                                        [ 📤 SUBMIT REVIEW ]     │
└─────────────────────────────────────────────────────────────────────────────────────────────────┘
```

1. Go to **My Orders** > **Delivered Orders**.
2. Click **Write a Review** next to the delivered product.
3. Select your **Star Rating (1 to 5)**.
4. Type your **Review Title** and detailed feedback.
5. Click **Submit Review**. Once verified by moderation, your review will be published on the product page.

---

## 8. Customer Support & FAQ

> [!TIP]
> Need immediate assistance? Contact our 24/7 support line at `support@merxo.com` or live chat in the lower right corner of the website.

**Q: How long does refund processing take for cancelled orders?**  
A: For prepaid Razorpay orders (UPI, Card, NetBanking), refunds are credited back to your original source account within **5 to 7 business days**.

**Q: What if I receive a wrong or damaged item?**  
A: Go to **My Orders** > **Order Details** > **Request Return / Replacement**. Upload a clear photo of the delivered item and box label within 7 days of delivery.

---

*Thank you for shopping with Merxo E-Commerce Platform!*
