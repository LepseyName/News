using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class QueueDictionary<T, K>
    {
        private T[] keys;
        private K[] values;

        private uint count;
        private int lenght;

        public QueueDictionary(uint Count)
        {
            this.count = Count;
            this.keys = new T[count];
            this.values = new K[count];
            this.lenght = 0;
        }


        public void addQueue(T key, K value)
        {
            if(this.lenght < this.count)
            {
                this.keys[this.lenght] = key;
                this.values[this.lenght] = value;
                this.lenght++;
            }
            else
            {
                T[] newKeys = new T[this.count];
                K[] newValues = new K[this.count];

                for(int i=0; i < this.count - 1; i++)
                {
                    newKeys[i] = this.keys[i + 1];
                    newValues[i] = this.values[i + 1];
                }

                newKeys[this.count-1] = key;
                newValues[this.count - 1] = value;

                this.keys = newKeys;
                this.values = newValues;
            }
        }

        public int indexInQueue(T key, K value)
        {
            for (int i = 0; i < this.lenght; i++)
                if (this.keys[i].Equals(key) && this.values[i].Equals(value))
                {
                    return i;
                }
            return -1;
        }

        public bool removeQueue(T key, K value)
        {
            int index = this.indexInQueue(key, value);
            if (index == -1)
                return false;


            this.lenght--;
            T[] newKeys = new T[this.lenght];
            K[] newValues = new K[this.lenght];

            for (int k = 0; k < index; k++)
            {
                newKeys[k] = this.keys[k];
                newValues[k] = this.values[k];
            }

            for (int k = index; k < this.lenght; k++)
            {
                newKeys[k] = this.keys[k + 1];
                newValues[k] = this.values[k + 1];
            }

            this.keys = newKeys;
            this.values = newValues;
            return true;
        }
    }
}
