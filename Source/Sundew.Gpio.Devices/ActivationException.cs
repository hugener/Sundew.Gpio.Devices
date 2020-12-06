// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActivationException.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Gpio.Devices
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception used to indicate that an activation should have been activated.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class ActivationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationException"/> class.
        /// </summary>
        public ActivationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ActivationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public ActivationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"></see> that contains contextual information about the source or destination.</param>
        protected ActivationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}