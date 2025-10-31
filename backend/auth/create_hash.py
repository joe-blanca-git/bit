import hashlib
import os
import sys

# Iterações de hash (DEVE SER O MESMO NÚMERO do security.py)
ITERATIONS = 100000

def create_new_hash(password):
    # 1. Cria um "salt" criptográfico aleatório
    salt = os.urandom(16)  # 16 bytes de salt
    
    # 2. Cria o hash usando a senha e o salt
    hashed_password = hashlib.pbkdf2_hmac(
        'sha256',
        password.encode('utf-8'),
        salt,
        ITERATIONS
    )
    
    # 3. Converte para hex e junta no formato "salt$hash"
    final_hash_string = f"{salt.hex()}${hashed_password.hex()}"
    
    print("\n--- SEU NOVO HASH (salt$hash) ---")
    print(final_hash_string)
    print("----------------------------------\n")
    print("Copie este hash e cole no seu banco de dados.")

if __name__ == "__main__":
    if len(sys.argv) > 1:
        password_to_hash = sys.argv[1]
        create_new_hash(password_to_hash)
    else:
        print("Erro: Você precisa passar a senha.")
        print("Exemplo: python create_hash.py admin")