[System.Serializable]
public struct MechProperty {
    public int HP, EN, SP, MPU;
    public int ENOutputRate;
    public int MinENRequired;
    public int Size, Weight;
    public int EnergyDrain;

    public int MaxHeat, CooldownRate;
    public int Marksmanship;

    public int ScanRange;

    public int VerticalBoostSpeed;
    public int BasicSpeed { set; private get; }
    public int Capacity;
    public int Deceleration;

    public int DashOutput { private get; set; }
    public int DashENDrain;
    public int JumpENDrain { private get; set; }

    private float DashAcceleration, DashDecelleration;

    public float GetJumpENDrain(int totalWeight) {
        return JumpENDrain + totalWeight / 160f;//TODO : improve this
    }

    public float GetDashSpeed(int totalWeight) {
        return DashOutput * 1.8f - totalWeight * 0.004f; //DashOutput * 1.8f : max speed  ;  0.004 weight coefficient
    }

    public float GetMoveSpeed(int partWeight, int weaponWeight) {
        int cal_capacity = (Capacity > 195000) ? 195000 : Capacity;

        double x1 = 0.0001064 * cal_capacity + 190.2552f, x2 = -0.0000024659 * cal_capacity + 0.69024f;
        //Debug.Log("part weight : "+partWeight + " weapon Weight : "+weaponWeight);
        //Debug.Log("basic speed : "+BasicSpeed + " coeff x1 : "+x1+" , x2 : "+x2);
        return (float)(BasicSpeed - (partWeight * x2 + weaponWeight) / x1);
    }

    public float GetDashAcceleration(int totalWeight) {
        return GetDashSpeed(totalWeight) / 100f - 1;
    }

    public float GetDashDecelleration(int totalWeight) {
        return Deceleration / 10000f - (totalWeight - Deceleration) / 20000f;
    }
}