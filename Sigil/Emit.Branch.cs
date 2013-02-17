﻿using Sigil.Impl;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Sigil
{
    public partial class Emit<DelegateType>
    {
        /// <summary>
        /// Unconditionally branches to the given label.
        /// </summary>
        public Emit<DelegateType> Branch(Label label)
        {
            if (label == null)
            {
                throw new ArgumentNullException("label");
            }

            if (((IOwned)label).Owner != this)
            {
                FailOwnership(label);
            }

            UnusedLabels.Remove(label);

            TrackersAtBranches[label] = CurrentVerifier.Clone();

            BufferedILGenerator.UpdateOpCodeDelegate update;
            UpdateState(OpCodes.Br, label, StackTransition.None(), out update);

            Branches[Stack.Unique()] = Tuple.Create(label, IL.Index);

            BranchPatches[IL.Index] = Tuple.Create(label, update, OpCodes.Br);

            if (TrackersAtLabels.ContainsKey(label))
            {
                var partial = TrackersAtLabels[label];

                if (!partial.Incoming(CurrentVerifier))
                {
                    // TODO: Gotta do better than this, needs "what the hell happened" messaging
                    throw new Exception("Branch violates stack");
                }
            }

            CurrentVerifier = new VerifiableTracker(baseless: true);

            return this;
        }

        /// <summary>
        /// Unconditionally branches to the label with the given name.
        /// </summary>
        public Emit<DelegateType> Branch(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            return Branch(Labels[name]);
        }

        /// <summary>
        /// Pops two arguments from the stack, if both are equal branches to the given label.
        /// </summary>
        public Emit<DelegateType> BranchIfEqual(Label label)
        {
            if (label == null)
            {
                throw new ArgumentNullException("label");
            }

            if (((IOwned)label).Owner != this)
            {
                FailOwnership(label);
            }

            var top = Stack.Top(2);

            if (top == null)
            {
                FailStackUnderflow(2);
            }

            UnusedLabels.Remove(label);

            var transitions =
                new[] 
                {
                    new StackTransition(new [] { typeof(WildcardType), typeof(WildcardType) }, Type.EmptyTypes)
                };

            BufferedILGenerator.UpdateOpCodeDelegate update;
            UpdateState(OpCodes.Beq, label, transitions, out update, pop: 2);

            Branches[Stack.Unique()] = Tuple.Create(label, IL.Index);

            BranchPatches[IL.Index] = Tuple.Create(label, update, OpCodes.Beq);

            TrackersAtBranches[label] = CurrentVerifier.Clone();

            return this;
        }

        /// <summary>
        /// Pops two arguments from the stack, if both are equal branches to the label with the given name.
        /// </summary>
        public Emit<DelegateType> BranchIfEqual(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            return BranchIfEqual(Labels[name]);
        }

        /// <summary>
        /// Pops two arguments from the stack, if they are not equal (when treated as unsigned values) branches to the given label.
        /// </summary>
        public Emit<DelegateType> UnsignedBranchIfNotEqual(Label label)
        {
            if (label == null)
            {
                throw new ArgumentNullException("label");
            }

            if (((IOwned)label).Owner != this)
            {
                FailOwnership(label);
            }

            var top = Stack.Top(2);

            if (top == null)
            {
                FailStackUnderflow(2);
            }

            UnusedLabels.Remove(label);

            var transitions =
                new[] 
                {
                    new StackTransition(new [] { typeof(WildcardType), typeof(WildcardType) }, Type.EmptyTypes)
                };

            BufferedILGenerator.UpdateOpCodeDelegate update;
            UpdateState(OpCodes.Bne_Un, label, transitions, out update, pop: 2);

            Branches[Stack.Unique()] = Tuple.Create(label, IL.Index);

            BranchPatches[IL.Index] = Tuple.Create(label, update, OpCodes.Bne_Un);

            TrackersAtBranches[label] = CurrentVerifier.Clone();

            return this;
        }

        /// <summary>
        /// Pops two arguments from the stack, if they are not equal (when treated as unsigned values) branches to the label with the given name.
        /// </summary>
        public Emit<DelegateType> UnsignedBranchIfNotEqual(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            return UnsignedBranchIfNotEqual(Labels[name]);
        }

        private IEnumerable<StackTransition> BranchComparableTransitions()
        {
            return
                new[]
                {
                    new StackTransition(new [] { typeof(int), typeof(int) }, Type.EmptyTypes),
                    new StackTransition(new [] { typeof(int), typeof(NativeIntType) }, Type.EmptyTypes),
                    new StackTransition(new [] { typeof(NativeIntType), typeof(int) }, Type.EmptyTypes),
                    new StackTransition(new [] { typeof(NativeIntType), typeof(NativeIntType) }, Type.EmptyTypes),
                    new StackTransition(new [] { typeof(long), typeof(long) }, Type.EmptyTypes),
                    new StackTransition(new [] { typeof(float), typeof(float) }, Type.EmptyTypes),
                    new StackTransition(new [] { typeof(double), typeof(double) }, Type.EmptyTypes)
                };
        }

        /// <summary>
        /// Pops two arguments from the stack, branches to the given label if the second value is greater than or equal to the first value.
        /// </summary>
        public Emit<DelegateType> BranchIfGreaterOrEqual(Label label)
        {
            if (label == null)
            {
                throw new ArgumentNullException("label");
            }

            if (((IOwned)label).Owner != this)
            {
                FailOwnership(label);
            }

            var top = Stack.Top(2);

            if (top == null)
            {
                FailStackUnderflow(2);
            }

            UnusedLabels.Remove(label);
            BufferedILGenerator.UpdateOpCodeDelegate update;

            UpdateState(OpCodes.Bge, label, BranchComparableTransitions(), out update, pop: 2);

            Branches[Stack.Unique()] = Tuple.Create(label, IL.Index);

            BranchPatches[IL.Index] = Tuple.Create(label, update, OpCodes.Bge);

            TrackersAtBranches[label] = CurrentVerifier.Clone();

            return this;
        }

        /// <summary>
        /// Pops two arguments from the stack, branches to the label with the given name if the second value is greater than or equal to the first value.
        /// </summary>
        public Emit<DelegateType> BranchIfGreaterOrEqual(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            return BranchIfGreaterOrEqual(Labels[name]);
        }

        /// <summary>
        /// Pops two arguments from the stack, branches to the given label if the second value is greater than or equal to the first value (when treated as unsigned values).
        /// </summary>
        public Emit<DelegateType> UnsignedBranchIfGreaterOrEqual(Label label)
        {
            if (label == null)
            {
                throw new ArgumentNullException("label");
            }

            if (((IOwned)label).Owner != this)
            {
                FailOwnership(label);
            }

            var top = Stack.Top(2);

            if (top == null)
            {
                FailStackUnderflow(2);
            }

            UnusedLabels.Remove(label);

            BufferedILGenerator.UpdateOpCodeDelegate update;

            UpdateState(OpCodes.Bge_Un, label, BranchComparableTransitions(), out update, pop: 2);

            Branches[Stack.Unique()] = Tuple.Create(label, IL.Index);

            BranchPatches[IL.Index] = Tuple.Create(label, update, OpCodes.Bge_Un);

            TrackersAtBranches[label] = CurrentVerifier.Clone();

            return this;
        }

        /// <summary>
        /// Pops two arguments from the stack, branches to the label with the given name if the second value is greater than or equal to the first value (when treated as unsigned values).
        /// </summary>
        public Emit<DelegateType> UnsignedBranchIfGreaterOrEqual(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            return UnsignedBranchIfGreaterOrEqual(Labels[name]);
        }

        /// <summary>
        /// Pops two arguments from the stack, branches to the given label if the second value is greater than the first value.
        /// </summary>
        public Emit<DelegateType> BranchIfGreater(Label label)
        {
            if (label == null)
            {
                throw new ArgumentNullException("label");
            }

            if (((IOwned)label).Owner != this)
            {
                FailOwnership(label);
            }

            var top = Stack.Top(2);

            if (top == null)
            {
                FailStackUnderflow(2);
            }

            UnusedLabels.Remove(label);

            BufferedILGenerator.UpdateOpCodeDelegate update;

            UpdateState(OpCodes.Bgt, label, BranchComparableTransitions(), out update, pop: 2);

            Branches[Stack.Unique()] = Tuple.Create(label, IL.Index);

            BranchPatches[IL.Index] = Tuple.Create(label, update, OpCodes.Bgt);

            TrackersAtBranches[label] = CurrentVerifier.Clone();

            return this;
        }

        /// <summary>
        /// Pops two arguments from the stack, branches to the label with the given name if the second value is greater than the first value.
        /// </summary>
        public Emit<DelegateType> BranchIfGreater(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            return BranchIfGreater(Labels[name]);
        }

        /// <summary>
        /// Pops two arguments from the stack, branches to the given label if the second value is greater than the first value (when treated as unsigned values).
        /// </summary>
        public Emit<DelegateType> UnsignedBranchIfGreater(Label label)
        {
            if (label == null)
            {
                throw new ArgumentNullException("label");
            }

            if (((IOwned)label).Owner != this)
            {
                FailOwnership(label);
            }

            var top = Stack.Top(2);

            if (top == null)
            {
                FailStackUnderflow(2);
            }

            UnusedLabels.Remove(label);

            BufferedILGenerator.UpdateOpCodeDelegate update;

            UpdateState(OpCodes.Bgt_Un, label, BranchComparableTransitions(), out update, pop: 2);

            Branches[Stack.Unique()] = Tuple.Create(label, IL.Index);

            BranchPatches[IL.Index] = Tuple.Create(label, update, OpCodes.Bgt_Un);

            TrackersAtBranches[label] = CurrentVerifier.Clone();

            return this;
        }

        /// <summary>
        /// Pops two arguments from the stack, branches to the label with the given name if the second value is greater than the first value (when treated as unsigned values).
        /// </summary>
        public Emit<DelegateType> UnsignedBranchIfGreater(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            return UnsignedBranchIfGreater(Labels[name]);
        }

        /// <summary>
        /// Pops two arguments from the stack, branches to the given label if the second value is less than or equal to the first value.
        /// </summary>
        public Emit<DelegateType> BranchIfLessOrEqual(Label label)
        {
            if (label == null)
            {
                throw new ArgumentNullException("label");
            }

            if (((IOwned)label).Owner != this)
            {
                FailOwnership(label);
            }

            var top = Stack.Top(2);

            if (top == null)
            {
                FailStackUnderflow(2);
            }

            UnusedLabels.Remove(label);

            BufferedILGenerator.UpdateOpCodeDelegate update;

            UpdateState(OpCodes.Ble, label, BranchComparableTransitions(), out update, pop: 2);

            Branches[Stack.Unique()] = Tuple.Create(label, IL.Index);

            BranchPatches[IL.Index] = Tuple.Create(label, update, OpCodes.Ble);

            TrackersAtBranches[label] = CurrentVerifier.Clone();

            return this;
        }

        /// <summary>
        /// Pops two arguments from the stack, branches to the label with the given name if the second value is less than or equal to the first value.
        /// </summary>
        public Emit<DelegateType> BranchIfLessOrEqual(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            return BranchIfLessOrEqual(Labels[name]);
        }

        /// <summary>
        /// Pops two arguments from the stack, branches to the given label if the second value is less than or equal to the first value (when treated as unsigned values).
        /// </summary>
        public Emit<DelegateType> UnsignedBranchIfLessOrEqual(Label label)
        {
            if (label == null)
            {
                throw new ArgumentNullException("label");
            }

            if (((IOwned)label).Owner != this)
            {
                FailOwnership(label);
            }

            var top = Stack.Top(2);

            if (top == null)
            {
                FailStackUnderflow(2);
            }

            UnusedLabels.Remove(label);

            BufferedILGenerator.UpdateOpCodeDelegate update;

            UpdateState(OpCodes.Ble_Un, label, BranchComparableTransitions(), out update, pop: 2);

            Branches[Stack.Unique()] = Tuple.Create(label, IL.Index);

            BranchPatches[IL.Index] = Tuple.Create(label, update, OpCodes.Ble_Un);

            TrackersAtBranches[label] = CurrentVerifier.Clone();

            return this;
        }

        /// <summary>
        /// Pops two arguments from the stack, branches to the label with the given name if the second value is less than or equal to the first value (when treated as unsigned values).
        /// </summary>
        public Emit<DelegateType> UnsignedBranchIfLessOrEqual(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            return UnsignedBranchIfLessOrEqual(Labels[name]);
        }

        /// <summary>
        /// Pops two arguments from the stack, branches to the given label if the second value is less than the first value.
        /// </summary>
        public Emit<DelegateType> BranchIfLess(Label label)
        {
            if (label == null)
            {
                throw new ArgumentNullException("label");
            }

            if (((IOwned)label).Owner != this)
            {
                FailOwnership(label);
            }

            var top = Stack.Top(2);

            if (top == null)
            {
                FailStackUnderflow(2);
            }

            UnusedLabels.Remove(label);

            BufferedILGenerator.UpdateOpCodeDelegate update;

            UpdateState(OpCodes.Blt, label, BranchComparableTransitions(), out update, pop: 2);

            Branches[Stack.Unique()] = Tuple.Create(label, IL.Index);

            BranchPatches[IL.Index] = Tuple.Create(label, update, OpCodes.Blt);

            TrackersAtBranches[label] = CurrentVerifier.Clone();

            return this;
        }

        /// <summary>
        /// Pops two arguments from the stack, branches to the label with the given name if the second value is less than the first value.
        /// </summary>
        public Emit<DelegateType> BranchIfLess(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            return BranchIfLess(Labels[name]);
        }

        /// <summary>
        /// Pops two arguments from the stack, branches to the given label if the second value is less than the first value (when treated as unsigned values).
        /// </summary>
        public Emit<DelegateType> UnsignedBranchIfLess(Label label)
        {
            if (label == null)
            {
                throw new ArgumentNullException("label");
            }

            if (((IOwned)label).Owner != this)
            {
                FailOwnership(label);
            }

            var top = Stack.Top(2);

            if (top == null)
            {
                FailStackUnderflow(2);
            }

            UnusedLabels.Remove(label);

            BufferedILGenerator.UpdateOpCodeDelegate update;

            UpdateState(OpCodes.Blt_Un, label, BranchComparableTransitions(), out update, pop: 2);

            Branches[Stack.Unique()] = Tuple.Create(label, IL.Index);

            BranchPatches[IL.Index] = Tuple.Create(label, update, OpCodes.Blt_Un);

            TrackersAtBranches[label] = CurrentVerifier.Clone();

            return this;
        }

        /// <summary>
        /// Pops two arguments from the stack, branches to the label with the given name if the second value is less than the first value (when treated as unsigned values).
        /// </summary>
        public Emit<DelegateType> UnsignedBranchIfLess(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            return UnsignedBranchIfLess(Labels[name]);
        }

        /// <summary>
        /// Pops one argument from the stack, branches to the given label if the value is false.
        /// 
        /// A value is false if it is zero or null.
        /// </summary>
        public Emit<DelegateType> BranchIfFalse(Label label)
        {
            if (label == null)
            {
                throw new ArgumentNullException("label");
            }

            if (((IOwned)label).Owner != this)
            {
                FailOwnership(label);
            }

            var top = Stack.Top(1);

            if (top == null)
            {
                FailStackUnderflow(1);
            }

            UnusedLabels.Remove(label);

            var transitions = 
                new []
                {
                    new StackTransition(new [] { typeof(WildcardType) }, Type.EmptyTypes)
                };

            BufferedILGenerator.UpdateOpCodeDelegate update;
            UpdateState(OpCodes.Brfalse, label, transitions, out update, pop: 1);

            Branches[Stack.Unique()] = Tuple.Create(label, IL.Index);

            BranchPatches[IL.Index] = Tuple.Create(label, update, OpCodes.Brfalse);

            TrackersAtBranches[label] = CurrentVerifier.Clone();

            return this;
        }

        /// <summary>
        /// Pops one argument from the stack, branches to the label with the given name if the value is false.
        /// 
        /// A value is false if it is zero or null.
        /// </summary>
        public Emit<DelegateType> BranchIfFalse(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            return BranchIfFalse(Labels[name]);
        }

        /// <summary>
        /// Pops one argument from the stack, branches to the given label if the value is true.
        /// 
        /// A value is true if it is non-zero or non-null.
        /// </summary>
        public Emit<DelegateType> BranchIfTrue(Label label)
        {
            if (label == null)
            {
                throw new ArgumentNullException("label");
            }

            if (((IOwned)label).Owner != this)
            {
                FailOwnership(label);
            }

            var top = Stack.Top(1);

            if (top == null)
            {
                FailStackUnderflow(1);
            }

            UnusedLabels.Remove(label);

            var transitions =
                new[]
                {
                    new StackTransition(new [] { typeof(WildcardType) }, Type.EmptyTypes)
                };

            BufferedILGenerator.UpdateOpCodeDelegate update;
            UpdateState(OpCodes.Brtrue, label, transitions, out update, pop: 1);

            Branches[Stack.Unique()] = Tuple.Create(label, IL.Index);

            BranchPatches[IL.Index] = Tuple.Create(label, update, OpCodes.Brtrue);

            TrackersAtBranches[label] = CurrentVerifier.Clone();

            return this;
        }

        /// <summary>
        /// Pops one argument from the stack, branches to the label with the given name if the value is true.
        /// 
        /// A value is true if it is non-zero or non-null.
        /// </summary>
        public Emit<DelegateType> BranchIfTrue(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            return BranchIfTrue(Labels[name]);
        }
    }
}
