import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface RazorpayConfigDto {
  keyId: string;
  accountEmail: string;
}

export interface CreateRazorpayOrderRequestDto {
  amount: number;
  currency: string;
  receipt?: string;
}

export interface CreateRazorpayOrderResponseDto {
  razorpayOrderId: string;
  keyId: string;
  amountInSubunits: number;
  currency: string;
  accountEmail: string;
}

export interface VerifyRazorpayPaymentDto {
  razorpayOrderId: string;
  razorpayPaymentId: string;
  razorpaySignature: string;
}

export interface RazorpayResponse {
  razorpay_payment_id: string;
  razorpay_order_id: string;
  razorpay_signature: string;
}

@Injectable({
  providedIn: 'root'
})
export class RazorpayPaymentService {
  private apiUrl = `${environment.apiUrl}/payments/razorpay`;

  constructor(private http: HttpClient) {}

  getConfig(): Observable<RazorpayConfigDto> {
    return this.http.get<RazorpayConfigDto>(`${this.apiUrl}/config`);
  }

  createOrder(dto: CreateRazorpayOrderRequestDto): Observable<CreateRazorpayOrderResponseDto> {
    return this.http.post<CreateRazorpayOrderResponseDto>(`${this.apiUrl}/create-order`, dto);
  }

  verifyPayment(dto: VerifyRazorpayPaymentDto): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(`${this.apiUrl}/verify`, dto);
  }

  openRazorpayCheckout(
    orderData: CreateRazorpayOrderResponseDto,
    userEmail: string = 'suthary980@gmail.com',
    userName: string = 'Customer',
    userContact: string = ''
  ): Promise<RazorpayResponse> {
    return new Promise((resolve, reject) => {
      if (typeof (window as any).Razorpay === 'undefined') {
        reject(new Error('Razorpay SDK is not loaded. Please check your internet connection.'));
        return;
      }

      const options = {
        key: orderData.keyId || 'rzp_test_suthary980',
        amount: orderData.amountInSubunits,
        currency: orderData.currency || 'INR',
        name: 'MerxoSell Marketplace',
        description: 'Secure Checkout Payment',
        image: 'assets/logo.png',
        order_id: orderData.razorpayOrderId,
        prefill: {
          name: userName,
          email: userEmail || orderData.accountEmail || 'suthary980@gmail.com',
          contact: userContact
        },
        theme: {
          color: '#fa6338'
        },
        handler: (response: RazorpayResponse) => {
          resolve(response);
        },
        modal: {
          ondismiss: () => {
            reject(new Error('Payment cancelled by user.'));
          }
        }
      };

      const rzp = new (window as any).Razorpay(options);
      rzp.on('payment.failed', (response: any) => {
        reject(new Error(response.error?.description || 'Payment failed. Please try again.'));
      });
      rzp.open();
    });
  }
}
