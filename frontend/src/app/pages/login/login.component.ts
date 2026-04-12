import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);

  username = '';
  password = '';
  errorMessage = '';
  isSubmitting = false;

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      void this.router.navigate(['/dashboard']);
    }
  }

  async login(): Promise<void> {
    this.errorMessage = '';

    const username = this.username.trim();
    const password = this.password;

    if (!username || !password) {
      this.errorMessage = 'Username and password are required.';
      return;
    }

    this.isSubmitting = true;

    try {
      await firstValueFrom(this.authService.login(username, password));
      await this.router.navigate(['/dashboard']);
    } catch {
      this.errorMessage = 'Login failed.';
    } finally {
      this.isSubmitting = false;
    }
  }
}
