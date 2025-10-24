import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'https://localhost:7086/api'; // Adjust the port if necessary

  constructor(private http: HttpClient) { }

  register(user: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/Users/AddUser`, user);
  }

  login(credentials: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/Users/Login`, credentials).pipe(
      tap((response: any) => {
        if (response.data.token) {
          this.storeToken(response.data.token);
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem('authToken');
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  getToken(): string | null {
    return localStorage.getItem('authToken');
  }

  getUsers(): Observable<any> {
    return this.http.get(`${this.apiUrl}/Users/GetAll`);
  }

  getUser(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/Users/GetById/${id}`);
  }

  updateProfile(id: string, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/Users/UpdateProfile/${id}`, data);
  }

  getDecodedToken(): any {
    const token = this.getToken();
    if (token) {
      return jwtDecode(token);
    }
    return null;
  }

  private storeToken(token: string): void {
    localStorage.setItem('authToken', token);
  }
}
