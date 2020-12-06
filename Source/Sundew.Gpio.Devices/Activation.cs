// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Activation.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Gpio.Devices
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Maintains an activation state.
    /// </summary>
    public sealed class Activation
    {
        private readonly Func<bool> isActivated;
        private readonly Action activateAction;
        private readonly Action deactivateAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="Activation"/> class.
        /// </summary>
        /// <param name="isActivated">The is activated.</param>
        /// <param name="activateAction">The activate action.</param>
        /// <param name="deactivateAction">The deactivate action.</param>
        /// <param name="activate">if set to <c>true</c> [activate].</param>
        public Activation(Func<bool> isActivated, Action activateAction, Action deactivateAction, bool activate)
        {
            this.isActivated = isActivated;
            this.deactivateAction = deactivateAction;
            this.activateAction = activateAction;
            this.SetActivation(activate);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is activated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is activated; otherwise, <c>false</c>.
        /// </value>
        public bool IsActivated => this.isActivated();

        /// <summary>
        /// Sets the activation.
        /// </summary>
        /// <param name="activate">if set to <c>true</c> [activate].</param>
        /// <returns>A value indicating whether the operation was successful.</returns>
        public bool SetActivation(bool activate)
        {
            var isActivated = this.IsActivated;
            if (activate && !isActivated)
            {
                this.activateAction();
                return true;
            }

            if (isActivated)
            {
                this.deactivateAction();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Verifies the activated.
        /// </summary>
        /// <exception cref="Sundew.Gpio.Devices.ActivationException">Exception thrown if activation is not activated.</exception>
        public void VerifyActivated([DoesNotReturnIf(true)]bool value)
        {
            if (value)
            {
                throw new ActivationException("SetActivation must be called with true.");
            }
        }
    }
}