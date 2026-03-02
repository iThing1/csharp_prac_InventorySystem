namespace csharp_prac_interface
{
    // 피격 가능한 모든 객체가 구현해야 하는 인터페이스.
    public interface IDamageable
    {
        int Hp { get; set; }
        int MaxHp { get; set; }
        int Def { get; set; }
        bool IsAlive { get; }

        // Def를 고려한 실제 피해 계산 후 Hp를 감소.

        void TakeDamage(int damage);
    }
}
