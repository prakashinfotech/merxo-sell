import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SellerDashboard } from './seller-dashboard';

describe('SellerDashboard', () => {
  let component: SellerDashboard;
  let fixture: ComponentFixture<SellerDashboard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SellerDashboard],
    }).compileComponents();

    fixture = TestBed.createComponent(SellerDashboard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
