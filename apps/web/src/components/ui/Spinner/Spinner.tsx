import classNames from 'classnames'
import type { CommonProps } from '../@types/common'
import type { ElementType, ComponentProps, Ref } from 'react'

export interface SpinnerProps extends CommonProps {
    indicator?: ElementType
    isSpining?: boolean
    size?: string | number
    enableTheme?: boolean
    variant?: 'arc' | 'segments' | 'bars'
    ref?: Ref<HTMLElement>
}

const ArcIcon = ({ height, width, ...rest }: ComponentProps<ElementType>) => (
    <svg
        height={height}
        width={width}
        viewBox="0 0 16 16"
        fill="none"
        aria-hidden="true"
        {...rest}
    >
        <circle
            cx="8"
            cy="8"
            r="6"
            stroke="currentColor"
            strokeOpacity="0.15"
            strokeWidth="3"
        ></circle>
        <path
            d="M14 8a6 6 0 0 1-9 5.196"
            stroke="currentColor"
            strokeWidth="3"
            strokeLinecap="round"
        ></path>
    </svg>
)

const SEGMENTS = 12

const SegmentsIcon = ({
    height,
    width,
    ...rest
}: ComponentProps<ElementType>) => (
    <svg
        height={height}
        width={width}
        viewBox="0 0 16 16"
        fill="currentColor"
        aria-hidden="true"
        {...rest}
    >
        {Array.from({ length: SEGMENTS }, (_, i) => (
            <rect
                key={i}
                x="7.1"
                y="0.5"
                width="1.8"
                height="4"
                rx="0.9"
                opacity={(i + 1) / SEGMENTS}
                transform={`rotate(${i * (360 / SEGMENTS)} 8 8)`}
            />
        ))}
    </svg>
)

const BARS = 5

const BarsIcon = ({ height, width, ...rest }: ComponentProps<ElementType>) => (
    <svg
        height={height}
        width={width}
        viewBox="0 0 16 16"
        fill="currentColor"
        aria-hidden="true"
        {...rest}
    >
        {Array.from({ length: BARS }, (_, i) => (
            <rect
                key={i}
                className="spinner-bar"
                x={0.5 + i * 3.25}
                y="0"
                width="2"
                height="16"
                rx="0.5"
                style={{ animationDelay: `${-1.2 + i * 0.1}s` }}
            />
        ))}
    </svg>
)

const INDICATORS: Record<NonNullable<SpinnerProps['variant']>, ElementType> = {
    arc: ArcIcon,
    segments: SegmentsIcon,
    bars: BarsIcon,
}

const getAnimationClass = (variant: SpinnerProps['variant']) => {
    switch (variant) {
        case 'segments':
            return 'animate-[spin_1s_steps(12)_infinite]'
        case 'bars':
            return undefined
        case 'arc':
        default:
            return 'animate-spin'
    }
}

const Spinner = (props: SpinnerProps) => {
    const {
        className,
        enableTheme = false,
        variant = 'arc',
        indicator: Component = INDICATORS[variant],
        isSpining = true,
        size = 20,
        style,
        ref,
        ...rest
    } = props

    const spinnerStyle = {
        height: size,
        width: size,
        ...style,
    }

    const spinnerClass = classNames(
        'spinner',
        enableTheme && 'spinner-default-theme',
        isSpining ? getAnimationClass(variant) : 'spinner-paused',
        className,
    )

    return (
        <Component
            ref={ref}
            style={spinnerStyle}
            className={spinnerClass}
            height={size}
            width={size}
            {...rest}
        />
    )
}

export default Spinner
