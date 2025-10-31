from pydantic import BaseModel
from typing import List, Optional

class ClaimBase(BaseModel):
    ClaimType: str
    ClaimValue: str

class Claim(ClaimBase):
    ClaimId:int
    IdUser: int
    
    class Config:
        from_attributes = True
    
class UserBase(BaseModel):
    UserName: str
    Email: Optional[str] = None
    FullName: Optional[str] = None

class User(UserBase):
    IdUser: int
    claims: List[Claim] = []

    class Config:
        from_attributes = True

class Token(BaseModel):
    access_token: str
    token_type: str
    expires_in: int

class TokenData(BaseModel):
    UserName: Optional[str] = None
    claims: Optional[list] = None

class UserData(BaseModel):
    name: str
    email: Optional[str] = None
    document: Optional[str] = None
    company: Optional[str] = None
    userType: Optional[str] = None
    claims: List[ClaimBase] = []

class LoginResponse(BaseModel):
    token: Token
    user: UserData

class UserLogin(BaseModel):
    username: str
    password: str