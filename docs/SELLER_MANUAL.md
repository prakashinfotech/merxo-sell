# Seller Operations & Portal User Manual

Welcome to the **Merxo E-Commerce Platform** Seller Guide. This manual provides complete instructions for registering as a seller, setting up your digital storefront, listing products, managing variants and inventory, fulfilling customer orders, and analyzing store performance.

---

## Table of Contents

1. [Seller Onboarding & Registration](#1-seller-onboarding--registration)
   - [Creating a Seller Account](#creating-a-seller-account)
   - [Store Profile Configuration](#store-profile-configuration)
   - [Payout & Financial Settings](#payout--financial-settings)
2. [Seller Dashboard & Performance Analytics](#2-seller-dashboard--performance-analytics)
   - [Dashboard Metrics Overview](#dashboard-metrics-overview)
   - [Sales & Revenue Reports](#sales--revenue-reports)
   - [Inventory & Stock Alerts](#inventory--stock-alerts)
3. [Product Catalog Management](#3-product-catalog-management)
   - [Creating New Product Listings](#creating-new-product-listings)
   - [Adding Product Variants (Colors & Sizes)](#adding-product-variants-colors--sizes)
   - [Uploading Product Images & Media](#uploading-product-images--media)
   - [Updating Prices & Stock Levels](#updating-prices--stock-levels)
4. [Product Moderation & Approval Workflow](#4-product-moderation--approval-workflow)
   - [Understanding Listing Statuses](#understanding-listing-statuses)
   - [Handling Rejections & Resubmitting](#handling-rejections--resubmitting)
5. [Order Processing & Fulfillment](#5-order-processing--fulfillment)
   - [Viewing Seller Orders](#viewing-seller-orders)
   - [Fulfillment Status Lifecycle](#fulfillment-status-lifecycle)
   - [Assigning Courier Tracking Numbers](#assigning-courier-tracking-numbers)
   - [Managing Cancellations](#managing-cancellations)
6. [Managing Customer Reviews & Store Rating](#6-managing-customer-reviews--store-rating)
7. [Seller Support & FAQ](#7-seller-support--faq)

---

## 1. Seller Onboarding & Registration

### Creating a Seller Account
1. Click **Become a Seller** on the platform footer or navigation bar.
2. Complete the Seller Registration form with your **Business Name**, **Owner Name**, **Email Address**, **Phone Number**, and password.
3. Submit the registration. Your application will enter the **Pending Admin Approval** queue.
4. Once verified and approved by the Platform Administrator, you will receive login credentials to access the Seller Portal (`/seller`).

### Store Profile Configuration
1. Log in to the Seller Portal and navigate to **Settings** > **Store Profile**.
2. **Store Details**: Enter your public Store Name, Store Description, Customer Service Email, and Phone Number.
3. **Store Branding**: Upload your Store Logo and Header Banner.
4. **Business Address**: Enter your warehouse or physical business location for return shipping purposes.

### Payout & Financial Settings
1. Go to **Settings** > **Financial Details**.
2. Enter your **Bank Account Name**, **Account Number**, **IFSC / Routing Code**, and **Bank Name**.
3. Save changes. Payouts for completed orders will be processed directly to this registered bank account.

---

## 2. Seller Dashboard & Performance Analytics

### Dashboard Metrics Overview
Upon logging into `/seller/dashboard`, your main control panel provides real-time performance summaries:
- **Total Revenue**: Cumulative revenue earned from delivered customer orders.
- **Total Orders**: Count of orders received across all statuses.
- **Active Products**: Number of published, approved live products.
- **Pending Approvals**: Products currently awaiting admin moderation review.

### Sales & Revenue Reports
- **Revenue Charts**: Visual charts showing sales trends daily, weekly, or monthly.
- **Top Selling Products**: Rankings of your highest-performing product listings by quantity and revenue.

### Inventory & Stock Alerts
- **Low Stock Notification Banner**: Automatically flags products where remaining inventory drops below 5 units.
- **Out of Stock Warnings**: Products with zero stock are highlighted so you can quickly update quantities or mark items out of stock.

---

## 3. Product Catalog Management

### Creating New Product Listings
1. Navigate to **Products** > **Add New Product** (`/seller/products/new`).
2. **Basic Information**:
   - **Product Name**: Enter a clear, descriptive title.
   - **Category & Subcategory**: Select appropriate categories (e.g., *Fashion > Men's Clothing*).
   - **Brand / Manufacturer**: Choose or enter the brand.
   - **Short & Full Description**: Provide detailed feature lists, materials, and care instructions.
3. **Pricing & Stock**:
   - **Base Price**: Regular retail price.
   - **Discount Price / Sale Price**: (Optional) Promotional pricing.
   - **SKU Code**: Stock Keeping Unit code for inventory tracking.
   - **Initial Stock Quantity**: Number of units available in warehouse.
4. Click **Save Draft** or **Submit for Approval**.

### Adding Product Variants (Colors & Sizes)
For products available in multiple configurations:
1. In the product editor, enable the **Has Variants** toggle.
2. **Color Swatches**: Add available color options (e.g., *Red*, *Blue*, *Black*).
3. **Sizes**: Add available size options (e.g., *S*, *M*, *L*, *XL*).
4. **Variant Matrix**: Specify individual SKU, additional price delta, and stock count for each specific combination (e.g., *Blue - Size M*).

### Uploading Product Images & Media
- Click **Upload Images** in the Media section.
- Upload high-resolution images (recommended resolution: 1000x1000 pixels, JPG/PNG).
- Drag images to reorder. The first image will act as the **Primary / Thumbnail** image across catalog search results.

### Updating Prices & Stock Levels
1. Go to **Products** > **Product List** (`/seller/products`).
2. Search for the product or SKU.
3. Edit inline or click **Edit** to update price or stock numbers instantly. Click **Save Changes**.

---

## 4. Product Moderation & Approval Workflow

### Understanding Listing Statuses
Every product passes through a moderation control flow to maintain catalog quality:

```
[ Draft ] ──> [ Pending Approval ] ──> [ Approved & Published ]
                       │
                       └──> [ Rejected (With Feedback) ]
```

| Status | Meaning | Live on Site? | Action Required |
| :--- | :--- | :---: | :--- |
| **Draft** | Listing is incomplete or saved for later editing. | No | Complete listing and submit. |
| **Pending Approval** | Submitted to Admin moderation queue. | No | Await admin review (24–48 hrs). |
| **Approved / Published** | Product verified and active. | **Yes** | Monitor stock and orders. |
| **Rejected** | Failed quality checks or policy compliance. | No | Read admin feedback and correct listing. |

### Handling Rejections & Resubmitting
1. If a product is rejected, its status will show **Rejected** with an admin note (e.g., *Blurry primary image* or *Incorrect category*).
2. Click **Edit Product**, correct the flagged issue, and click **Resubmit for Approval**.

---

## 5. Order Processing & Fulfillment

### Viewing Seller Orders
1. Navigate to **Orders** (`/seller/orders`).
2. View all customer orders that contain items from your store.
3. Filter orders by status: *Pending*, *Processing*, *Shipped*, or *Delivered*.

### Fulfillment Status Lifecycle
As a seller, you are responsible for updating order fulfillment stages promptly:

1. **Pending**: New customer order received. Click **Accept & Process Order** to change status to **Processing**.
2. **Processing**: Pick, pack, and label the item in your warehouse.
3. **Shipped**: Package handed over to delivery logistics partner. Click **Mark as Shipped**.
4. **Delivered**: Courier completes delivery. Status updates to **Delivered**.

> [!IMPORTANT]
> Mark orders as **Processing** within 24 hours of placement to maintain high seller performance metrics.

### Assigning Courier Tracking Numbers
- When marking an order as **Shipped**:
  1. Select the **Courier / Logistics Partner** (e.g., *BlueDart, FedEx, Delhivery*).
  2. Enter the **Tracking / AWB Number**.
  3. Click **Confirm Shipment**. The buyer will automatically receive tracking updates.

### Managing Cancellations
- If a buyer cancels an order prior to shipment, you will receive a notification and the order status will change to **Cancelled**.
- Inventory stock associated with the cancelled items will automatically revert back to your available product inventory.

---

## 6. Managing Customer Reviews & Store Rating

1. Navigate to **Reviews** (`/seller/reviews`).
2. View customer feedback, star ratings, and comments left on your products.
3. Use customer feedback to improve product descriptions, sizing guides, and packaging quality.
4. Maintain an overall store rating above **4.0 Stars** to qualify for featured placement on homepage banners.

---

## 7. Seller Support & FAQ

**Q: When do I receive payout for delivered orders?**  
A: Payouts are generated automatically on a weekly/bi-weekly schedule for orders marked as **Delivered** past the 7-day return window.

**Q: Can I temporarily pause my store while on vacation?**  
A: Yes. Go to **Settings** > **Store Profile** and switch **Store Status** to *Vacation Mode*. This will temporarily hide your listings from search without deleting them.

**Q: What happens if an item runs out of stock?**  
A: The system automatically marks the product as *Out of Stock* on the buyer portal to prevent over-selling. Replenish your stock quantity in the Seller Portal to make it live again.

---

*Thank you for selling with Merxo E-Commerce Platform!*
