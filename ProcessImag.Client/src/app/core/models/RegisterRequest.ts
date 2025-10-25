export interface RegisterRequest {
  nume: string;
  email: string;
  parola: string;
  subscriptieId: number;
}
export interface RegisterResponse {
  message: string;
  utilizatorId: number;
}
