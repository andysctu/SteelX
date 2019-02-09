public interface IDamageableManager {//manage damageable components such that component can be find using spec id
    void RegisterDamageableComponent(IDamageable c);
    void DeregisterDamageableComponent(IDamageable c);
    IDamageable FindDamageableComponent(int specID);
}

