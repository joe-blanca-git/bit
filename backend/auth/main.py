from fastapi import Depends, FastAPI, HTTPException, status, Form
from fastapi.security import OAuth2PasswordRequestForm
from sqlalchemy.orm import Session, joinedload
from datetime import timedelta

from . import models
from . import schemas
from . import security
from .database import engine, get_db

models.Base.metadata.create_all(bind=engine)

app = FastAPI(
    title="Autenticação Sistema Bit",
    description="Sistema de autenticação de usuários para as aplicações Bit System",
    version = "0.0.1"
)



# --- FUNÇÕES DE LÓGICA ---

def get_user_by_username(db: Session, username: str):
    return db.query(models.User).options(
        joinedload(models.User.claims),
        joinedload(models.User.enterprise_data),
        joinedload(models.User.people_data).joinedload(models.People.type_info)
        
    ).filter(models.User.username == username).first()

class LoginForm:
    def __init__(
        self,
        username: str = Form(...),
        password: str = Form(...)
    ):
        self.username = username
        self.password = password

# --- ENDPOINTS DA API ---
# Em main.py

# ... (imports) ...

# 1. MUDE O RESPONSE_MODEL para o novo schema aninhado
@app.post("/token", response_model=schemas.LoginResponse) 
def login_for_access_token(
    form_data: LoginForm = Depends(),
    db: Session = Depends(get_db)
):
    user = get_user_by_username(db, username=form_data.username)

    if not user or not security.verify_password(form_data.password, user.password):
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Usuário ou senha incorretos.",
            headers={"WWW-Authenticate": "Bearer"},
        )
    
    # --- Cria o token (como antes) ---
    UserClaims =[{"type": claim.ClaimType, "value": claim.ClaimValue} for claim in user.claims]
    access_token_expires = timedelta(minutes=security.ACCESS_TOKEN_EXPIRE_MINUTES)
    access_token = security.create_access_token(
        data={"sub": user.username, "claims": UserClaims}, 
        expires_delta=access_token_expires
    )
    
    # --- 3. CONSTRUA OS OBJETOS ANINHADOS (COM SEGURANÇA) ---
    
    # Cria o objeto do Token
    token_data = schemas.Token(
        access_token=access_token,
        token_type="bearer",
        expires_in=int(access_token_expires.total_seconds())
    )
    
    # (O 'user' agora tem 'user.people_data' e 'user.enterprise_data')
    # Adicionamos checagens (ex: 'if user.people_data else None') 
    # para evitar erros se um usuário não tiver uma empresa ou 'people'
    
    user_data = schemas.UserData(
        name=user.fullname, # (Vem da tabela 'users')
        email=user.email,   # (Vem da tabela 'users')
        
        # (Vem da tabela 'people')
        document=user.people_data.cpf if user.people_data else None,
        
        # (Vem da tabela 'people_type' através da 'people')
        userType=user.people_data.type_info.name if user.people_data and user.people_data.type_info else None,
        
        # (Vem da tabela 'enterprise')
        company=user.enterprise_data.name if user.enterprise_data else None,
        
        # (Vem da tabela 'claims')
        claims=[{"ClaimType": c.ClaimType, "ClaimValue": c.ClaimValue} for c in user.claims]    )

    # 4. RETORNE O OBJETO COMPLETO
    return schemas.LoginResponse(token=token_data, user=user_data)