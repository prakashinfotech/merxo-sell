import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { SellerShell } from './seller-shell';
import { AuthService } from '../../../core/auth/auth.service';
import { ProfileService } from '../../../core/profile/profile.service';

describe('SellerShell', () => {
  let component: SellerShell;
  let fixture: ComponentFixture<SellerShell>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SellerShell],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: { isLoggedIn: () => false, logout: () => {} } },
        { provide: ProfileService, useValue: { getProfile: () => of({ fullName: 'Test Seller', email: 'seller@test.com' }) } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(SellerShell);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
