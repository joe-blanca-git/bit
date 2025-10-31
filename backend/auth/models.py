from sqlalchemy import Column, Integer, String, ForeignKey
from sqlalchemy.orm import relationship
from .database import Base

class PeopleType(Base):
    __tablename__ = "people_type"
    id = Column(Integer, primary_key=True) 
    name = Column(String(255))

class Enterprise(Base):
    __tablename__ = "enterprise"
    idEnterprise = Column(Integer, primary_key=True) 
    name = Column(String(255))
    idUser = Column(Integer, ForeignKey("users.IdUser")) 

    user = relationship("User", back_populates="enterprise_data")

class People(Base):
    __tablename__ = "people"
    idPeople = Column(Integer, primary_key=True) 
    cpf = Column(String(50))
    idUser = Column(Integer, ForeignKey("users.IdUser")) 
    idPeopleType = Column(Integer, ForeignKey("people_type.id"))

    user = relationship("User", back_populates="people_data")

    type_info = relationship("PeopleType")

class User(Base):
    __tablename__ = "users"

    IdUser = Column(Integer, primary_key=True, index=True)
    username = Column(String(255), unique=True, index=True)
    password = Column(String(255))
    fullname = Column(String(255))
    email = Column(String(255))
    
    claims = relationship("Claims", back_populates="owner")
    people_data = relationship("People", back_populates="user", uselist=False)
    enterprise_data = relationship("Enterprise", back_populates="user", uselist=False)

class Claims(Base):
    __tablename__ = "claims"

    ClaimId = Column(Integer, primary_key=True, index=True)
    ClaimType = Column(String(255), index=True)
    ClaimValue = Column(String(255), index=True)
    IdUser = Column(Integer, ForeignKey("users.IdUser"))

    owner = relationship("User", back_populates="claims")