import os
import hashlib
import hmac  # <--- 1. ADICIONE ESTE IMPORT
from datetime import datetime, timedelta
from typing import Optional
from jose import JWTError, jwt
from dotenv import load_dotenv

load_dotenv()

SECRET_KEY = os.getenv("SECRET_KEY")
ALGORITHM = os.getenv("ALGORITHM")
ACCESS_TOKEN_EXPIRE_MINUTES = int(os.getenv("ACCESS_TOKEN_EXPIRE_MINUTES"))

ITERATIONS = 100000

def verify_password(plain_password: str, stored_password_hash: str) -> bool:
    try:
        salt_hex, hash_hex = stored_password_hash.split('$')
        
        salt = bytes.fromhex(salt_hex)
        
        recalculated_hash = hashlib.pbkdf2_hmac(
            'sha256',
            plain_password.encode('utf-8'),
            salt,
            ITERATIONS
        )
        
        hash_to_check = bytes.fromhex(hash_hex)
        
        # 5. Compara os dois.
        return hmac.compare_digest(recalculated_hash, hash_to_check) # <--- 2. MUDE DE 'hashlib' PARA 'hmac'
        
    except Exception as e:
        print(f"Erro ao verificar hash: {e}")
        return False

def create_access_token(data: dict, expires_delta: Optional[timedelta] = None):
    # ... (esta função não muda)
    to_encode = data.copy()
    if expires_delta:
        expire = datetime.utcnow() + expires_delta
    else:
        expire = datetime.utcnow() + timedelta(minutes=ACCESS_TOKEN_EXPIRE_MINUTES)
    
    to_encode.update({"exp": expire})
    encoded_jwt = jwt.encode(to_encode, SECRET_KEY, algorithm=ALGORITHM)
    return encoded_jwt