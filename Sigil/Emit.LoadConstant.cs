﻿using Sigil.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Sigil
{
    public partial class Emit<DelegateType>
    {
        /// <summary>
        /// Push a constant integer onto the stack.
        /// </summary>
        public void LoadConstant(int i)
        {
            switch (i)
            {
                case -1: UpdateState(OpCodes.Ldc_I4_M1, TypeOnStack.Get<int>()); return;
                case 0: UpdateState(OpCodes.Ldc_I4_0, TypeOnStack.Get<int>()); return;
                case 1: UpdateState(OpCodes.Ldc_I4_1, TypeOnStack.Get<int>()); return;
                case 2: UpdateState(OpCodes.Ldc_I4_2, TypeOnStack.Get<int>()); return;
                case 3: UpdateState(OpCodes.Ldc_I4_3, TypeOnStack.Get<int>()); return;
                case 4: UpdateState(OpCodes.Ldc_I4_4, TypeOnStack.Get<int>()); return;
                case 5: UpdateState(OpCodes.Ldc_I4_5, TypeOnStack.Get<int>()); return;
                case 6: UpdateState(OpCodes.Ldc_I4_6, TypeOnStack.Get<int>()); return;
                case 7: UpdateState(OpCodes.Ldc_I4_7, TypeOnStack.Get<int>()); return;
                case 8: UpdateState(OpCodes.Ldc_I4_8, TypeOnStack.Get<int>()); return;
            }

            if (i >= byte.MinValue && i <= byte.MaxValue)
            {
                UpdateState(OpCodes.Ldc_I4_S, i, TypeOnStack.Get<int>());
                return;
            }

            UpdateState(OpCodes.Ldc_I4, i, TypeOnStack.Get<int>());
        }

        public void LoadConstant(long l)
        {
            UpdateState(OpCodes.Ldc_I8, l, TypeOnStack.Get<long>());
        }

        public void LoadConstant(float f)
        {
            UpdateState(OpCodes.Ldc_R4, f, TypeOnStack.Get<float>());
        }

        public void LoadConstant(double d)
        {
            UpdateState(OpCodes.Ldc_R8, d, TypeOnStack.Get<double>());
        }

        public void LoadConstant(string str)
        {
            UpdateState(OpCodes.Ldstr, str, TypeOnStack.Get<string>());
        }

        public void LoadConstant(FieldInfo field)
        {
            UpdateState(OpCodes.Ldtoken, field, TypeOnStack.Get<RuntimeFieldHandle>());
        }

        public void LoadConstant(MethodInfo method)
        {
            UpdateState(OpCodes.Ldtoken, method, TypeOnStack.Get<RuntimeMethodHandle>());
        }

        public void LoadConstant(TypeInfo type)
        {
            UpdateState(OpCodes.Ldtoken, type, TypeOnStack.Get<RuntimeTypeHandle>());
        }

        public void LoadNull()
        {
            UpdateState(OpCodes.Ldnull, TypeOnStack.Get<object>());
        }
    }
}
