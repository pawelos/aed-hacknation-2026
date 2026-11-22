import CloseButton from '../CloseButton'
import { DialogContext, useDialogContext, useDialog } from '../hooks/useDialog'
import useWindowSize from '../hooks/useWindowSize'
import classNames from '../utils/classNames'
import {
    useMergeRefs,
    FloatingPortal,
    FloatingFocusManager,
    FloatingOverlay,
    useTransitionStyles,
} from '@floating-ui/react'
import type { CommonProps } from '../@types/common'
import { CSSProperties, Ref, Suspense } from 'react'
import Spinner from '../Spinner'

export interface DialogProps extends CommonProps {
    closable?: boolean
    height?: string | number
    isOpen?: boolean
    lockScroll?: boolean
    onClose?: (close: boolean) => void
    onOpen?: (open: boolean) => void
    overlayClassName?: string
    shouldCloseOnOverlayClick?: boolean
    shouldCloseOnEsc?: boolean
    width?: number
    ref?: Ref<HTMLDivElement>
    /** Id of the element that names the window (its heading), for screen readers. */
    labelledBy?: string
    /** Accessible name of the close button (e.g. "Zamknij"). */
    closeLabel?: string
}

const DialogContent = (props: DialogProps) => {
    const {
        closable = true,
        height,
        children,
        width = 520,
        lockScroll = false,
        ref: propRef,
        labelledBy,
        closeLabel,
        onClose,
        onOpen: _onOpen,
        overlayClassName,
        className,
        isOpen: _isOpen,
        shouldCloseOnOverlayClick: _shouldCloseOnOverlayClick,
        shouldCloseOnEsc: _shouldCloseOnEsc,
        ...rest
    } = props
    const currentSize = useWindowSize()
    const { context: floatingContext, ...context } = useDialogContext()
    const ref = useMergeRefs([context.refs.setFloating, propRef])

    const contentStyle: CSSProperties = {}

    if (width !== undefined) {
        contentStyle.width = width

        if (
            typeof currentSize.width !== 'undefined' &&
            currentSize.width <= width
        ) {
            contentStyle.width = 'auto'
        }
    }

    const backdropTransition = useTransitionStyles(floatingContext, {
        duration: { open: 150, close: 150 },
    })

    const floatingTransition = useTransitionStyles(floatingContext, {
        duration: { open: 200, close: 150 },
        initial: {
            opacity: 0,
            transform: 'scale(0.9)',
        },
        close: {
            opacity: 0,
            transform: 'scale(0.9)',
        },
    })

    const renderCloseButton = (
        <CloseButton
            absolute
            className="ltr:right-4 rtl:left-4 top-4 hover:bg-gray-100 hover:dark:bg-gray-700 rounded-lg"
            aria-label={closeLabel}
            onClick={() => onClose?.(context.open as boolean)}
        />
    )

    return (
        <FloatingPortal>
            {backdropTransition.isMounted && (
                <>
                    <FloatingOverlay
                        className={classNames(
                            'dialog-overlay',
                            overlayClassName,
                        )}
                        lockScroll={lockScroll}
                        style={backdropTransition.styles}
                    />
                    <div className="dialog-content-wrapper">
                        <FloatingFocusManager context={floatingContext}>
                            <div
                                className={classNames(
                                    'dialog-content',
                                    className,
                                )}
                                style={{
                                    height,
                                    ...contentStyle,
                                    ...floatingTransition.styles,
                                }}
                                ref={ref}
                                aria-labelledby={labelledBy ?? context.labelId}
                                aria-describedby={context.descriptionId}
                                {...context.getFloatingProps(rest)}
                            >
                                {/* Lazily loaded content waits here, not in the page's loading.tsx (which hides the open dialog). */}
                                <Suspense
                                    fallback={
                                        <div className="flex justify-center py-8">
                                            <Spinner size={28} />
                                        </div>
                                    }
                                >
                                    {children}
                                </Suspense>
                                {closable && renderCloseButton}
                            </div>
                        </FloatingFocusManager>
                    </div>
                </>
            )}
        </FloatingPortal>
    )
}

const Dialog = ({
    isOpen,
    shouldCloseOnOverlayClick = true,
    shouldCloseOnEsc = true,
    ...props
}: DialogProps) => {
    const dialog = useDialog({
        open: isOpen,
        onOpenChange: (open: boolean) => {
            if (open) {
                props.onOpen?.(open)
            } else {
                props.onClose?.(open)
            }
        },
        shouldCloseOnOverlayClick: shouldCloseOnOverlayClick,
        shouldCloseOnEsc: shouldCloseOnEsc,
    })
    return (
        <DialogContext.Provider value={dialog}>
            <DialogContent {...props} />
        </DialogContext.Provider>
    )
}

export default Dialog
