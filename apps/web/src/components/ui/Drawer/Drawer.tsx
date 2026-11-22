import CloseButton from '../CloseButton'
import { DialogContext, useDialogContext, useDialog } from '../hooks/useDialog'
import classNames from '../utils/classNames'
import {
    useMergeRefs,
    FloatingPortal,
    FloatingFocusManager,
    FloatingOverlay,
    useTransitionStyles,
} from '@floating-ui/react'
import type { CommonProps } from '../@types/common'
import { CSSProperties, Ref, ReactNode, Suspense } from 'react'
import Spinner from '../Spinner'

export interface DrawerProps extends CommonProps {
    bodyClass?: string
    closable?: boolean
    contentClassName?: string
    footer?: string | ReactNode
    footerClass?: string
    headerClass?: string
    height?: string | number
    isOpen?: boolean
    lockScroll?: boolean
    onClose?: (close: boolean) => void
    onOpen?: (open: boolean) => void
    overlayClassName?: string
    placement?: 'top' | 'right' | 'bottom' | 'left'
    shouldCloseOnOverlayClick?: boolean
    shouldCloseOnEsc?: boolean
    width?: number | string
    title?: string | ReactNode
    ref?: Ref<HTMLDivElement>
    /** Id of the element that names the window (its heading), for screen readers. */
    labelledBy?: string
    /** Accessible name of the close button (e.g. "Zamknij"). */
    closeLabel?: string
}

const DEFAULT_SIZE = 370

const DrawerContent = (props: DrawerProps) => {
    const {
        bodyClass,
        closable = true,
        contentClassName,
        height = DEFAULT_SIZE,
        headerClass,
        footerClass,
        children,
        footer,
        lockScroll = true,
        ref: propRef,
        labelledBy,
        closeLabel,
        onClose,
        overlayClassName,
        placement = 'right',
        width = DEFAULT_SIZE,
        title,
        ...rest
    } = props
    const { context: floatingContext, ...context } = useDialogContext()
    const ref = useMergeRefs([context.refs.setFloating, propRef])

    const contentStyle: CSSProperties = {}

    const backdropTransition = useTransitionStyles(floatingContext, {
        duration: { open: 150, close: 150 },
    })

    const getPlacementStyle = (
        state: 'initial' | 'close' | 'open',
    ): CSSProperties => {
        let value = ''

        const isHorizontal = placement === 'left' || placement === 'right'
        const isVertical = placement === 'top' || placement === 'bottom'

        if (state === 'open') {
            return {
                [placement]: 0,
                ...(isHorizontal ? { width } : { height }),
            }
        }

        if (state === 'close' || state === 'initial') {
            value = '-'
        }

        if (isHorizontal) {
            return {
                // A number is in pixels, a string keeps its unit ("100vw").
                [placement]: `${value}${typeof width === 'number' ? `${width}px` : width}`,
                width: width,
            }
        }

        if (isVertical) {
            return {
                // A number is in pixels, a string keeps its unit ("100vw").
                [placement]: `${value}${typeof height === 'number' ? `${height}px` : height}`,
                height: height,
            }
        }

        return {}
    }

    const getPlacementClass = () => {
        const classes: string[] = []

        if (placement === 'left' || placement === 'right') {
            classes.push('horizontal')
        }

        if (placement === 'top' || placement === 'bottom') {
            classes.push('vertical')
        }

        classes.push('drawer-floating')
        classes.push(`drawer-floating-${placement}`)

        return classes.join(' ')
    }

    const floatingTransition = useTransitionStyles(floatingContext, {
        duration: { open: 250, close: 250 },
        initial: {
            opacity: 0,
            ...getPlacementStyle('initial'),
        },
        open: {
            opacity: 1,
            ...getPlacementStyle('open'),
        },
        close: {
            opacity: 0,
            ...getPlacementStyle('close'),
        },
    })

    const renderCloseButton = (
        <CloseButton
            absolute
            className="ltr:right-4 rtl:left-4 top-3 hover:bg-gray-100 hover:dark:bg-gray-700 rounded-lg"
            aria-label={closeLabel}
            onClick={() => onClose?.(context.open as boolean)}
        />
    )

    const isHorizontal = placement === 'left' || placement === 'right'

    return (
        <FloatingPortal>
            {backdropTransition.isMounted && (
                <>
                    <FloatingOverlay
                        className={classNames(
                            'drawer-overlay',
                            overlayClassName,
                        )}
                        lockScroll={lockScroll}
                        style={backdropTransition.styles}
                    />
                    <div className="drawer-content-wrapper">
                        <FloatingFocusManager context={floatingContext}>
                            <div
                                className={classNames(
                                    'drawer-content',
                                    getPlacementClass(),
                                    contentClassName,
                                )}
                                style={{
                                    ...contentStyle,
                                    ...floatingTransition.styles,
                                    ...(isHorizontal ? { width } : { height }),
                                }}
                                ref={ref}
                                aria-labelledby={labelledBy ?? context.labelId}
                                aria-describedby={context.descriptionId}
                                {...context.getFloatingProps(rest)}
                            >
                                {title || closable ? (
                                    <div
                                        className={classNames(
                                            'drawer-header',
                                            headerClass,
                                        )}
                                    >
                                        {typeof title === 'string' ? (
                                            <h5>{title}</h5>
                                        ) : (
                                            // A div, so a custom title may hold a heading.
                                            <div className="min-w-0">
                                                {title}
                                            </div>
                                        )}
                                        {closable && renderCloseButton}
                                    </div>
                                ) : null}
                                <div
                                    className={classNames(
                                        'drawer-body',
                                        bodyClass,
                                    )}
                                >
                                    {/* Lazily loaded content waits here, not in the page's loading.tsx (which hides the open drawer). */}
                                    <Suspense
                                        fallback={
                                            <div className="flex justify-center py-8">
                                                <Spinner size={28} />
                                            </div>
                                        }
                                    >
                                        {children}
                                    </Suspense>
                                </div>
                                {footer && (
                                    <div
                                        className={classNames(
                                            'drawer-footer',
                                            footerClass,
                                        )}
                                    >
                                        {footer}
                                    </div>
                                )}
                            </div>
                        </FloatingFocusManager>
                    </div>
                </>
            )}
        </FloatingPortal>
    )
}

const Drawer = ({
    isOpen,
    shouldCloseOnOverlayClick,
    shouldCloseOnEsc,
    ...props
}: DrawerProps) => {
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
            <DrawerContent {...props} />
        </DialogContext.Provider>
    )
}

export default Drawer
