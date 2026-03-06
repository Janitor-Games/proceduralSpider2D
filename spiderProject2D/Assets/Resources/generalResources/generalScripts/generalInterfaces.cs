public interface damageInterface 
{
    float currentHealth {get;}
    float maxHealth {get;}
    void setCurrent(float val);
    void hurtThing();
    void killThing();
}

public interface hurtInterface
{
    void doHurt();
}

public interface hitInterface
{
    void doHit();
}
