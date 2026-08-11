import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../../core/auth/auth.service';
import { Router } from '@angular/router';
import { ProfileService } from '../../../core/profile/profile.service';

@Component({
  selector: 'app-seller-shell',
  templateUrl: './seller-shell.html',
  styleUrls: ['./seller-shell.scss'],
  standalone: false,
})
export class SellerShell implements OnInit {
  userName  = '';
  userEmail = '';
  isSidebarCollapsed = false;

  toggleSidebar(): void { this.isSidebarCollapsed = !this.isSidebarCollapsed; }

  constructor(
    public auth: AuthService,
    private router: Router,
    private profileService: ProfileService,
  ) {}

  ngOnInit(): void {
    this.profileService.getProfile().subscribe({
      next: p => {
        this.userName  = p.fullName;
        this.userEmail = p.email;
      },
    });
  }

  getInitials(): string {
    if (!this.userName) return 'S';
    return this.userName.split(' ').map(w => w[0]).join('').toUpperCase().slice(0, 2);
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/auth/seller/login']);
  }
}
