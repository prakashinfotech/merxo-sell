import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AuthService } from '../../core/auth/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-admin',
  templateUrl: './admin.html',
  styleUrls: ['./admin.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Admin {
  isSidebarCollapsed = false;

  constructor(public auth: AuthService, private router: Router) {}

  toggleSidebar() {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/auth/admin/login']);
  }
}
