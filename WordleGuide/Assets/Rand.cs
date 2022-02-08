using System;

public static class Rand
{
    public static int RandomRange(UInt32 seed, int min, int max)
    {
        // Shift around
        seed ^= seed << 21;
        seed ^= seed >> 35;
        seed ^= seed << 4;

        // Convert the value to our range
        int value = (int)(seed % max);

        // Return the value
        return value;
    }
}
