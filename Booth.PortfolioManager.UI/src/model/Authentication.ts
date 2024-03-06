export interface AuthenticationRequest {
	userName: string;
	password: string;
}

export interface AuthenticationResponse {
	token: string;
}